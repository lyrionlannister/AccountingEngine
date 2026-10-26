using Accounting.Domain;
using Accounting.Domain.Enums;

namespace Accounting.Tests;

public class AuditTrailEntryTests
{
    private static AccountingPeriod SamplePeriod() =>
        AccountingPeriod.Create(1, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

    [Fact]
    public void ForAccountingPeriod_SetsOnlyThatReference()
    {
        var period = SamplePeriod();

        var entry = AuditTrailEntry.ForAccountingPeriod(period, AuditOperation.PeriodClosed, 1, "user-1", DateTimeOffset.UtcNow);

        Assert.Same(period, entry.AccountingPeriod);
        Assert.Null(entry.JournalEntry);
        Assert.Null(entry.PeriodReopenRequest);
        Assert.Null(entry.AccountBalanceSnapshot);
        Assert.Equal(AuditOperation.PeriodClosed, entry.Operation);
        Assert.Equal("user-1", entry.Actor);
    }

    [Fact]
    public void ForPeriodReopenRequest_WithoutActor_Throws()
    {
        var request = PeriodReopenRequest.Create(1, 1, "Corrección", "user-1", DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(() =>
            AuditTrailEntry.ForPeriodReopenRequest(request, AuditOperation.PeriodReopenRequested, 1, " ", DateTimeOffset.UtcNow));
    }
}
