using Accounting.Domain.Enums;
using Accounting.DTOs;

namespace Accounting.Services;

public sealed class ReverseJournalEntryService
{
    private readonly IJournalEntryRepository _journalEntries;
    private readonly IAccountingPeriodRepository _periods;
    private readonly IVoucherTypeRepository _voucherTypes;
    private readonly IAuditTrailWriter _auditTrail;

    public ReverseJournalEntryService(
        IJournalEntryRepository journalEntries,
        IAccountingPeriodRepository periods,
        IVoucherTypeRepository voucherTypes,
        IAuditTrailWriter auditTrail)
    {
        _journalEntries = journalEntries;
        _periods = periods;
        _voucherTypes = voucherTypes;
        _auditTrail = auditTrail;
    }

    public async Task<JournalEntryResult> ReverseAsync(ReverseJournalEntryRequest request, CancellationToken cancellationToken)
    {
        var original = await _journalEntries.FindByIdAsync(request.CompanyId, request.JournalEntryId, cancellationToken)
            ?? throw new InvalidOperationException($"Journal entry {request.JournalEntryId} not found.");

        var period = await _periods.FindByIdAsync(request.CompanyId, original.AccountingPeriodId, cancellationToken)
            ?? throw new InvalidOperationException($"Accounting period {original.AccountingPeriodId} not found.");
        if (period.Status != PeriodStatus.Open)
            throw new AccountingPeriodClosedException(period.Id);
        if (!period.Contains(request.EntryDate))
            throw new ArgumentException("Entry date is outside the accounting period.", nameof(request));

        var reversal = original.Reverse(request.EntryDate, request.Description);

        _auditTrail.Record(AuditOperation.JournalEntryReversed, request.CompanyId, request.PostedBy, reversal, DateTimeOffset.UtcNow);

        await JournalEntryPoster.PostAsync(_journalEntries, _voucherTypes, reversal, request.SequenceNumber, request.PostedBy, cancellationToken);

        return PostJournalEntryService.ToResult(reversal);
    }
}
