using Accounting.Domain;
using Accounting.Domain.Enums;
using Accounting.DTOs;

namespace Accounting.Services;

public sealed class CloseAccountingPeriodService
{
    private readonly IAccountingPeriodRepository _periods;
    private readonly IJournalEntryRepository _journalEntries;
    private readonly IAuditTrailWriter _auditTrail;

    public CloseAccountingPeriodService(IAccountingPeriodRepository periods, IJournalEntryRepository journalEntries, IAuditTrailWriter auditTrail)
    {
        _periods = periods;
        _journalEntries = journalEntries;
        _auditTrail = auditTrail;
    }

    public async Task<AccountingPeriodResult> CloseAsync(CloseAccountingPeriodRequest request, CancellationToken cancellationToken)
    {
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
