namespace Accounting.Domain;

public sealed class AccountBalanceSnapshot
{
    public long Id { get; private set; }
    public long AccountingPeriodId { get; private set; }
    public long AccountId { get; private set; }
    public decimal DebitTotal { get; private set; }
    public decimal CreditTotal { get; private set; }
    public DateTimeOffset CalculatedAt { get; private set; }

    private AccountBalanceSnapshot()
    {
    }

    public static AccountBalanceSnapshot Create(long accountingPeriodId, long accountId, decimal debitTotal, decimal creditTotal, DateTimeOffset calculatedAt)
    {
        var snapshot = new AccountBalanceSnapshot
        {
            AccountingPeriodId = accountingPeriodId,
            AccountId = accountId
        };
        snapshot.Recalculate(debitTotal, creditTotal, calculatedAt);
        return snapshot;
    }

    public void Recalculate(decimal debitTotal, decimal creditTotal, DateTimeOffset calculatedAt)
    {
        if (debitTotal < 0 || creditTotal < 0)
            throw new ArgumentException("Totals cannot be negative.");

        DebitTotal = debitTotal;
        CreditTotal = creditTotal;
        CalculatedAt = calculatedAt;
    }
}
