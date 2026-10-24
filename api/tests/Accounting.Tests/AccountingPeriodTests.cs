using Accounting.Domain;
using Accounting.Domain.Enums;

namespace Accounting.Tests;

public class AccountingPeriodTests
{
    [Fact]
    public void Create_WithValidData_SetsPropertiesAndIsOpen()
    {
        var period = AccountingPeriod.Create(1, "2026-10", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));

        Assert.Equal(PeriodStatus.Open, period.Status);
        Assert.Null(period.ClosedAt);
        Assert.Null(period.ClosedBy);
    }

    [Fact]
    public void Create_WithEndDateBeforeStartDate_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            AccountingPeriod.Create(1, "2026-10", new DateOnly(2026, 10, 31), new DateOnly(2026, 10, 1)));
    }

    [Fact]
    public void Close_WhenOpen_SetsClosedState()
    {
        var period = AccountingPeriod.Create(1, "2026-10", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));
        var now = DateTimeOffset.UtcNow;

        period.Close("user-1", now);

        Assert.Equal(PeriodStatus.Closed, period.Status);
        Assert.Equal(now, period.ClosedAt);
        Assert.Equal("user-1", period.ClosedBy);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_Throws()
    {
        var period = AccountingPeriod.Create(1, "2026-10", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));
        period.Close("user-1", DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => period.Close("user-1", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Reopen_WhenClosed_SetsOpenAndClearsClosedInfo()
    {
        var period = AccountingPeriod.Create(1, "2026-10", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));
        period.Close("user-1", DateTimeOffset.UtcNow);

        period.Reopen();

        Assert.Equal(PeriodStatus.Open, period.Status);
        Assert.Null(period.ClosedAt);
        Assert.Null(period.ClosedBy);
    }

    [Fact]
    public void Reopen_WhenOpen_Throws()
    {
        var period = AccountingPeriod.Create(1, "2026-10", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));

        Assert.Throws<InvalidOperationException>(() => period.Reopen());
    }

    [Fact]
    public void Contains_DateInsideRange_ReturnsTrue()
    {
        var period = AccountingPeriod.Create(1, "2026-10", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));

        Assert.True(period.Contains(new DateOnly(2026, 10, 15)));
        Assert.False(period.Contains(new DateOnly(2026, 11, 1)));
    }
}
