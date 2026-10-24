namespace Accounting.Domain;

public sealed class JournalEntryLine
{
    public long Id { get; private set; }
    public long JournalEntryId { get; private set; }
    public int LineNumber { get; private set; }
    public long AccountId { get; private set; }
    public long? CostCenterId { get; private set; }
    public long? PartyId { get; private set; }
    public long CurrencyId { get; private set; }
    public decimal DebitAmount { get; private set; }
    public decimal CreditAmount { get; private set; }
    public string? Description { get; private set; }

    private JournalEntryLine()
    {
    }

    public static JournalEntryLine Create(
        int lineNumber,
        long accountId,
        long? costCenterId,
        long? partyId,
        long currencyId,
        decimal debitAmount,
        decimal creditAmount,
        string? description)
    {
        if (debitAmount < 0 || creditAmount < 0)
            throw new ArgumentException("Amounts cannot be negative.");
        if (debitAmount > 0 == creditAmount > 0)
            throw new ArgumentException("Exactly one of debit or credit amount must be greater than zero.");

        return new JournalEntryLine
        {
            LineNumber = lineNumber,
            AccountId = accountId,
            CostCenterId = costCenterId,
            PartyId = partyId,
            CurrencyId = currencyId,
            DebitAmount = debitAmount,
            CreditAmount = creditAmount,
            Description = description
        };
    }

    internal JournalEntryLine Swapped(int lineNumber) =>
        Create(lineNumber, AccountId, CostCenterId, PartyId, CurrencyId, CreditAmount, DebitAmount, Description);
}
