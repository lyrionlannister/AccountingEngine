using Accounting.Domain;
using Accounting.Domain.Enums;

namespace Accounting.Services;

internal static class JournalEntryPoster
{
    public static async Task PostAsync(
        IJournalEntryRepository journalEntries,
        IVoucherTypeRepository voucherTypes,
        JournalEntry entry,
        long? manualSequenceNumber,
        string postedBy,
        CancellationToken cancellationToken)
    {
        var voucherType = await voucherTypes.FindByIdAsync(entry.CompanyId, entry.VoucherTypeId, cancellationToken)
            ?? throw new InvalidOperationException($"Voucher type {entry.VoucherTypeId} not found.");

        if (voucherType.NumberingMode == VoucherNumberingMode.Automatic)
        {
            await journalEntries.AddWithAutomaticSequenceAsync(entry, sequenceNumber =>
            {
                entry.AssignSequenceNumber(sequenceNumber);
                entry.Post(DateTimeOffset.UtcNow, postedBy);
            }, cancellationToken);
        }
        else
        {
            if (manualSequenceNumber is not { } sequenceNumber || sequenceNumber <= 0)
                throw new ArgumentException("A positive sequence number is required for manual numbering.", nameof(manualSequenceNumber));

            entry.AssignSequenceNumber(sequenceNumber);
            entry.Post(DateTimeOffset.UtcNow, postedBy);
            await journalEntries.AddAsync(entry, cancellationToken);
        }
    }
}
