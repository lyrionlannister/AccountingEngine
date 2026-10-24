using Accounting.Domain;

namespace Accounting.Tests;

public class AccountBalanceSnapshotTests
{
    [Fact]
    public void Create_SetsTotals()
    {
        var now = DateTimeOffset.UtcNow;
        var snapshot = AccountBalanceSnapshot.Create(1, 1, debitTotal: 300m, creditTotal: 100m, calculatedAt: now);

        Assert.Equal(300m, snapshot.DebitTotal);
        Assert.Equal(100m, snapshot.CreditTotal);
        Assert.Equal(now, snapshot.CalculatedAt);
    }

    [Fact]
    public void Recalculate_UpdatesTotals()
    {
        var snapshot = AccountBalanceSnapshot.Create(1, 1, 300m, 100m, DateTimeOffset.UtcNow);
        var later = DateTimeOffset.UtcNow.AddMinutes(5);

        snapshot.Recalculate(400m, 150m, later);

        Assert.Equal(400m, snapshot.DebitTotal);
        Assert.Equal(150m, snapshot.CreditTotal);
        Assert.Equal(later, snapshot.CalculatedAt);
    }

    [Fact]
    public void Recalculate_WithNegativeTotal_Throws()
    {
        var snapshot = AccountBalanceSnapshot.Create(1, 1, 0m, 0m, DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(() => snapshot.Recalculate(-1m, 0m, DateTimeOffset.UtcNow));
    }
}
