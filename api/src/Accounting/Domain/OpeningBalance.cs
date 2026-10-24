namespace Accounting.Domain;

public sealed class OpeningBalance
{
    public long Id { get; private set; }
    public long AccountingPeriodId { get; private set; }
    public long AccountId { get; private set; }
    public long CurrencyId { get; private set; }
    public decimal DebitAmount { get; private set; }
    public decimal CreditAmount { get; private set; }

    private OpeningBalance()
    {
    }

    public static OpeningBalance Create(long accountingPeriodId, long accountId, long currencyId, decimal debitAmount, decimal creditAmount)
    {
        if (debitAmount < 0 || creditAmount < 0)
            throw new ArgumentException("Amounts cannot be negative.");
        if (debitAmount > 0 == creditAmount > 0)
            throw new ArgumentException("Exactly one of debit or credit amount must be greater than zero.");

        return new OpeningBalance
        {
            AccountingPeriodId = accountingPeriodId,
            AccountId = accountId,
            CurrencyId = currencyId,
            DebitAmount = debitAmount,
            CreditAmount = creditAmount
        };
    }
}
