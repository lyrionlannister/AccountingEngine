using Accounting.Domain;

namespace Accounting.Tests;

public class JournalEntryLineTests
{
    [Fact]
    public void Create_WithOnlyDebit_Succeeds()
    {
        var line = JournalEntryLine.Create(1, accountId: 1, costCenterId: null, partyId: null, currencyId: 1, debitAmount: 100m, creditAmount: 0m, description: null);

        Assert.Equal(100m, line.DebitAmount);
        Assert.Equal(0m, line.CreditAmount);
    }

    [Fact]
    public void Create_WithBothDebitAndCredit_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            JournalEntryLine.Create(1, 1, null, null, 1, debitAmount: 100m, creditAmount: 100m, description: null));
    }

    [Fact]
    public void Create_WithNeitherDebitNorCredit_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            JournalEntryLine.Create(1, 1, null, null, 1, debitAmount: 0m, creditAmount: 0m, description: null));
    }

    [Fact]
    public void Create_WithNegativeAmount_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            JournalEntryLine.Create(1, 1, null, null, 1, debitAmount: -1m, creditAmount: 0m, description: null));
    }
}
