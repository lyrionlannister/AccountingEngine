using Accounting.Domain.Enums;
using Accounting.DTOs;
using Shared;

namespace Accounting.Services;

public sealed class ResolvePeriodReopenService
{
    private readonly IPeriodReopenRequestRepository _reopenRequests;
    private readonly IAccountingPeriodRepository _periods;
    private readonly IAuditTrailWriter _auditTrail;
    private readonly ICurrentCompanyContext _currentCompany;

    public ResolvePeriodReopenService(
        IPeriodReopenRequestRepository reopenRequests, IAccountingPeriodRepository periods, IAuditTrailWriter auditTrail, ICurrentCompanyContext currentCompany)
    {
        _reopenRequests = reopenRequests;
        _periods = periods;
        _auditTrail = auditTrail;
        _currentCompany = currentCompany;
    }

    public async Task<PeriodReopenRequestResult> ResolveAsync(ResolvePeriodReopenRequest request, CancellationToken cancellationToken)
    {
        RoleGuard.Require(_currentCompany.Role, "Admin");

        var reopenRequest = await _reopenRequests.FindByIdAsync(request.CompanyId, request.PeriodReopenRequestId, cancellationToken)
            ?? throw new InvalidOperationException($"Period reopen request {request.PeriodReopenRequestId} not found.");

        // Segregacion de funciones: ni un Admin puede aprobar/rechazar su propia solicitud.
        if (string.Equals(reopenRequest.RequestedBy, request.ResolvedBy, StringComparison.Ordinal))
            throw new SelfApprovalNotAllowedException();

        if (request.Approve)
        {
            reopenRequest.Approve(request.ResolvedBy, DateTimeOffset.UtcNow, request.Notes);

            var period = await _periods.FindByIdAsync(request.CompanyId, reopenRequest.AccountingPeriodId, cancellationToken)
                ?? throw new InvalidOperationException($"Accounting period {reopenRequest.AccountingPeriodId} not found.");
            period.Reopen();
        }
        else
        {
            reopenRequest.Reject(request.ResolvedBy, DateTimeOffset.UtcNow, request.Notes);
        }

        _auditTrail.Record(AuditOperation.PeriodReopenResolved, request.CompanyId, request.ResolvedBy, reopenRequest, DateTimeOffset.UtcNow, request.Notes);

        // Un solo SaveChangesAsync (en transaccion explicita) para la solicitud, el periodo
        // reabierto si aplica, y la auditoria — antes eran 2 SaveChangesAsync separados
        // (ver tasks/plan-auditoria.md, hallazgo de la seccion 2).
        await _reopenRequests.SaveResolutionAsync(cancellationToken);

        return RequestPeriodReopenService.ToResult(reopenRequest);
    }
}
