using Accounting.DTOs;

namespace Accounting.Services;

public sealed class ResolvePeriodReopenService
{
    private readonly IPeriodReopenRequestRepository _reopenRequests;
    private readonly IAccountingPeriodRepository _periods;

    public ResolvePeriodReopenService(IPeriodReopenRequestRepository reopenRequests, IAccountingPeriodRepository periods)
    {
        _reopenRequests = reopenRequests;
        _periods = periods;
    }

    public async Task<PeriodReopenRequestResult> ResolveAsync(ResolvePeriodReopenRequest request, CancellationToken cancellationToken)
    {
        var reopenRequest = await _reopenRequests.FindByIdAsync(request.CompanyId, request.PeriodReopenRequestId, cancellationToken)
            ?? throw new InvalidOperationException($"Period reopen request {request.PeriodReopenRequestId} not found.");

        if (request.Approve)
        {
            reopenRequest.Approve(request.ResolvedBy, DateTimeOffset.UtcNow, request.Notes);

            var period = await _periods.FindByIdAsync(request.CompanyId, reopenRequest.AccountingPeriodId, cancellationToken)
                ?? throw new InvalidOperationException($"Accounting period {reopenRequest.AccountingPeriodId} not found.");
            period.Reopen();
            await _periods.UpdateAsync(period, cancellationToken);
        }
        else
        {
            reopenRequest.Reject(request.ResolvedBy, DateTimeOffset.UtcNow, request.Notes);
        }

        await _reopenRequests.UpdateAsync(reopenRequest, cancellationToken);

        return RequestPeriodReopenService.ToResult(reopenRequest);
    }
}
