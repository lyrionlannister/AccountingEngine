using Accounting.Domain;
using Accounting.DTOs;

namespace Accounting.Services;

public sealed class RequestPeriodReopenService
{
    private readonly IAccountingPeriodRepository _periods;
    private readonly IPeriodReopenRequestRepository _reopenRequests;

    public RequestPeriodReopenService(IAccountingPeriodRepository periods, IPeriodReopenRequestRepository reopenRequests)
    {
        _periods = periods;
        _reopenRequests = reopenRequests;
    }

    public async Task<PeriodReopenRequestResult> RequestAsync(RequestPeriodReopenRequest request, CancellationToken cancellationToken)
    {
        var period = await _periods.FindByIdAsync(request.CompanyId, request.AccountingPeriodId, cancellationToken)
            ?? throw new InvalidOperationException($"Accounting period {request.AccountingPeriodId} not found.");
        if (period.Status != Domain.Enums.PeriodStatus.Closed)
            throw new InvalidOperationException("Only a closed period can have a reopen request.");

        var reopenRequest = PeriodReopenRequest.Create(request.CompanyId, request.AccountingPeriodId, request.Reason, request.RequestedBy, DateTimeOffset.UtcNow);
        await _reopenRequests.AddAsync(reopenRequest, cancellationToken);

        return ToResult(reopenRequest);
    }

    internal static PeriodReopenRequestResult ToResult(PeriodReopenRequest request) =>
        new(request.Id, request.PublicId, request.Status.ToString(), request.RequestedBy, request.RequestedAt, request.ResolvedBy, request.ResolvedAt, request.ResolutionNotes);
}
