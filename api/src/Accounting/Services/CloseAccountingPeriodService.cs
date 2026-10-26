using Accounting.Domain;
using Accounting.Domain.Enums;
using Accounting.DTOs;
using Shared;

namespace Accounting.Services;

public sealed class CloseAccountingPeriodService
{
    private readonly IAccountingPeriodRepository _periods;
    private readonly IJournalEntryRepository _journalEntries;
    private readonly IAuditTrailWriter _auditTrail;
    private readonly ICurrentCompanyContext _currentCompany;

    public CloseAccountingPeriodService(
        IAccountingPeriodRepository periods, IJournalEntryRepository journalEntries, IAuditTrailWriter auditTrail, ICurrentCompanyContext currentCompany)
    {
        _periods = periods;
        _journalEntries = journalEntries;
        _auditTrail = auditTrail;
        _currentCompany = currentCompany;
    }

    public async Task<AccountingPeriodResult> CloseAsync(CloseAccountingPeriodRequest request, CancellationToken cancellationToken)
    {
        // Cerrar un periodo es una operacion de mayor impacto que contabilizar/revertir --
        // reservada a Admin (hallazgo de auditoria: ningun rol se validaba antes de esto).
        RoleGuard.Require(_currentCompany.Role, "Admin");

        // El chequeo de borradores y el cierre corren DENTRO del lock de fila que CloseAsync
        // toma sobre el periodo (FOR UPDATE) — sin esto, un Post concurrente podia colarse
        // durante la ventana completa de esta decision, no solo durante el UPDATE final
        // (hallazgo de auditoria, api/CLAUDE.md §5.4).
        var period = await _periods.CloseAsync(request.CompanyId, request.AccountingPeriodId, async (period, ct) =>
        {
            // Ver AccountingPeriodHasDraftEntriesException: bloquea, no ignora (decisión documentada).
            if (await _journalEntries.HasDraftEntriesAsync(request.CompanyId, request.AccountingPeriodId, ct))
                throw new AccountingPeriodHasDraftEntriesException(request.AccountingPeriodId);

            period.Close(request.ClosedBy, DateTimeOffset.UtcNow);
            _auditTrail.Record(AuditOperation.PeriodClosed, request.CompanyId, request.ClosedBy, period, DateTimeOffset.UtcNow);
        }, cancellationToken);

        return ToResult(period);
    }

    internal static AccountingPeriodResult ToResult(AccountingPeriod period) =>
        new(period.Id, period.PublicId, period.Status.ToString(), period.ClosedAt, period.ClosedBy);
}
