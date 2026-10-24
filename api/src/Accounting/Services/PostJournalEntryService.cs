using Accounting.Domain;
using Accounting.Domain.Enums;
using Accounting.DTOs;

namespace Accounting.Services;

public sealed class PostJournalEntryService
{
    private readonly IJournalEntryRepository _journalEntries;
    private readonly IAccountingPeriodRepository _periods;
    private readonly IVoucherTypeRepository _voucherTypes;

    public PostJournalEntryService(
        IJournalEntryRepository journalEntries,
        IAccountingPeriodRepository periods,
        IVoucherTypeRepository voucherTypes)
    {
        _journalEntries = journalEntries;
        _periods = periods;
        _voucherTypes = voucherTypes;
    }

    public async Task<JournalEntryResult> PostAsync(PostJournalEntryRequest request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.SourceReference))
        {
            var existing = await _journalEntries.FindBySourceAsync(
                request.CompanyId, request.SourceModule, request.SourceReference, cancellationToken);

            if (existing is not null)
            {
                if (!MatchesRequest(existing, request))
                    throw new JournalEntryIdempotencyConflictException(request.SourceModule, request.SourceReference);

                return ToResult(existing);
            }
        }

        var period = await _periods.FindByIdAsync(request.CompanyId, request.AccountingPeriodId, cancellationToken)
            ?? throw new InvalidOperationException($"Accounting period {request.AccountingPeriodId} not found.");
        if (period.Status != PeriodStatus.Open)
            throw new AccountingPeriodClosedException(period.Id);
        if (!period.Contains(request.EntryDate))
            throw new ArgumentException("Entry date is outside the accounting period.", nameof(request));

        var entry = JournalEntry.Create(
            request.CompanyId, request.VoucherTypeId, request.AccountingPeriodId, request.EntryDate,
            request.Description, request.SourceModule, request.SourceReference);

        foreach (var line in request.Lines)
            entry.AddLine(line.AccountId, line.CostCenterId, line.PartyId, line.CurrencyId, line.DebitAmount, line.CreditAmount, line.Description);

        await JournalEntryPoster.PostAsync(_journalEntries, _voucherTypes, entry, request.SequenceNumber, request.PostedBy, cancellationToken);

        return ToResult(entry);
    }

    private static bool MatchesRequest(JournalEntry existing, PostJournalEntryRequest request)
    {
        if (existing.EntryDate != request.EntryDate || existing.Description != request.Description)
            return false;
        if (existing.Lines.Count != request.Lines.Count)
            return false;

        for (var i = 0; i < existing.Lines.Count; i++)
        {
            var existingLine = existing.Lines[i];
            var requestLine = request.Lines[i];
            if (existingLine.AccountId != requestLine.AccountId
                || existingLine.CostCenterId != requestLine.CostCenterId
                || existingLine.PartyId != requestLine.PartyId
                || existingLine.CurrencyId != requestLine.CurrencyId
                || existingLine.DebitAmount != requestLine.DebitAmount
                || existingLine.CreditAmount != requestLine.CreditAmount)
                return false;
        }

        return true;
    }

    internal static JournalEntryResult ToResult(JournalEntry entry) =>
        new(entry.Id, entry.PublicId, entry.SequenceNumber, entry.Status.ToString(), entry.PostedAt);
}
