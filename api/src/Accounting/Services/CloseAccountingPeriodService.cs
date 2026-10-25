using Accounting.Domain;
using Accounting.DTOs;

namespace Accounting.Services;

public sealed class CloseAccountingPeriodService
{
    private readonly IAccountingPeriodRepository _periods;
    private readonly IJournalEntryRepository _journalEntries;

    public CloseAccountingPeriodService(IAccountingPeriodRepository periods, IJournalEntryRepository journalEntries)
    {
        _periods = periods;
        _journalEntries = journalEntries;
    }

    public async Task<AccountingPeriodResult> CloseAsync(CloseAccountingPeriodRequest request, CancellationToken cancellationToken)
    {
        var period = await _periods.FindByIdAsync(request.CompanyId, request.AccountingPeriodId, cancellationToken)
            ?? throw new InvalidOperationException($"Accounting period {request.AccountingPeriodId} not found.");

        // Ver AccountingPeriodHasDraftEntriesException: bloquea, no ignora (decisión documentada).
        if (await _journalEntries.HasDraftEntriesAsync(request.CompanyId, request.AccountingPeriodId, cancellationToken))
            throw new AccountingPeriodHasDraftEntriesException(request.AccountingPeriodId);

        period.Close(request.ClosedBy, DateTimeOffset.UtcNow);
        await _periods.UpdateAsync(period, cancellationToken);

        return ToResult(period);
    }

    internal static AccountingPeriodResult ToResult(AccountingPeriod period) =>
        new(period.Id, period.PublicId, period.Status.ToString(), period.ClosedAt, period.ClosedBy);
}
