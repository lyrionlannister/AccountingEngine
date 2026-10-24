using Accounting.Domain.Enums;

namespace Accounting.Domain;

public sealed class JournalEntry
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long CompanyId { get; private set; }
    public long VoucherTypeId { get; private set; }
    public long AccountingPeriodId { get; private set; }
    public long? SequenceNumber { get; private set; }
    public JournalEntryStatus Status { get; private set; }
    public DateOnly EntryDate { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? SourceModule { get; private set; }
    public string? SourceReference { get; private set; }
    public long? ReversalOfEntryId { get; private set; }
    public DateTimeOffset? PostedAt { get; private set; }
    public string? PostedBy { get; private set; }

    private readonly List<JournalEntryLine> _lines = new();
    public IReadOnlyList<JournalEntryLine> Lines => _lines;

    private JournalEntry()
    {
    }

    public static JournalEntry Create(
        long companyId,
        long voucherTypeId,
        long accountingPeriodId,
        DateOnly entryDate,
        string description,
        string? sourceModule = null,
        string? sourceReference = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        return new JournalEntry
        {
            CompanyId = companyId,
            VoucherTypeId = voucherTypeId,
            AccountingPeriodId = accountingPeriodId,
            EntryDate = entryDate,
            Description = description,
            SourceModule = sourceModule,
            SourceReference = sourceReference,
            Status = JournalEntryStatus.Draft
        };
    }

    public void AddLine(
        long accountId,
        long? costCenterId,
        long? partyId,
        long currencyId,
        decimal debitAmount,
        decimal creditAmount,
        string? description = null)
    {
        if (Status != JournalEntryStatus.Draft)
            throw new InvalidOperationException("Lines can only be added to a draft entry.");

        _lines.Add(JournalEntryLine.Create(_lines.Count + 1, accountId, costCenterId, partyId, currencyId, debitAmount, creditAmount, description));
    }

    public void AssignSequenceNumber(long sequenceNumber)
    {
        if (Status != JournalEntryStatus.Draft)
            throw new InvalidOperationException("Sequence number can only be assigned to a draft entry.");
        if (sequenceNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(sequenceNumber), "Sequence number must be greater than zero.");

        SequenceNumber = sequenceNumber;
    }

    public void Post(DateTimeOffset now, string postedBy)
    {
        if (Status != JournalEntryStatus.Draft)
            throw new InvalidOperationException("Only a draft entry can be posted.");
        if (string.IsNullOrWhiteSpace(postedBy))
            throw new ArgumentException("PostedBy is required.", nameof(postedBy));
        if (_lines.Count < 2)
            throw new InvalidOperationException("A journal entry needs at least two lines.");
        if (_lines.Sum(l => l.DebitAmount) != _lines.Sum(l => l.CreditAmount))
            throw new InvalidOperationException("Debits and credits are not balanced.");

        Status = JournalEntryStatus.Posted;
        PostedAt = now;
        PostedBy = postedBy;
    }

    public void Void()
    {
        if (Status != JournalEntryStatus.Draft)
            throw new InvalidOperationException("Only a draft entry can be voided.");

        Status = JournalEntryStatus.Voided;
    }

    public JournalEntry Reverse(DateOnly entryDate, string description)
    {
        if (Status != JournalEntryStatus.Posted)
            throw new InvalidOperationException("Only a posted entry can be reversed.");

        var reversal = Create(CompanyId, VoucherTypeId, AccountingPeriodId, entryDate, description, SourceModule, SourceReference);
        reversal.ReversalOfEntryId = Id;
        foreach (var line in _lines)
            reversal._lines.Add(line.Swapped(reversal._lines.Count + 1));

        return reversal;
    }
}
