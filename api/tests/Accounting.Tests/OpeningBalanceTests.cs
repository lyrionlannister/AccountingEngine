using Accounting.Domain;

namespace Accounting.Tests;

public class OpeningBalanceTests
{
    [Fact]
    public void Create_WithOnlyDebit_Succeeds()
    {
        var balance = OpeningBalance.Create(1, 1, 1, debitAmount: 500m, creditAmount: 0m);

        Assert.Equal(500m, balance.DebitAmount);
        Assert.Equal(0m, balance.CreditAmount);
    }

    [Fact]
    public void Create_WithBothDebitAndCredit_Throws()
    {
        Assert.Throws<ArgumentException>(() => OpeningBalance.Create(1, 1, 1, 500m, 500m));
    }

    [Fact]
    public void Create_WithNeitherDebitNorCredit_Throws()
    {
        Assert.Throws<ArgumentException>(() => OpeningBalance.Create(1, 1, 1, 0m, 0m));
    }
}
