using Accounting.Domain;
using Accounting.DTOs;
using Accounting.Services;

namespace Accounting.Tests;

public class CloseAccountingPeriodServiceTests
{
    private const long CompanyId = 1;

    [Fact]
    public async Task CloseAsync_WhenAlreadyClosed_ThrowsInvalidOperationException()
    {
        var periods = new FakeAccountingPeriodRepository();
        var period = AccountingPeriod.Create(CompanyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        period.Close("first-closer", DateTimeOffset.UtcNow);
        periods.Periods.Add(period);

        var service = new CloseAccountingPeriodService(periods);
        var request = new CloseAccountingPeriodRequest(CompanyId, AccountingPeriodId: 0, ClosedBy: "second-closer");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CloseAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CloseAsync_WhenOpen_ClosesThePeriod()
    {
        var periods = new FakeAccountingPeriodRepository();
        var period = AccountingPeriod.Create(CompanyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        periods.Periods.Add(period);

        var service = new CloseAccountingPeriodService(periods);
        var request = new CloseAccountingPeriodRequest(CompanyId, AccountingPeriodId: 0, ClosedBy: "closer");

        var result = await service.CloseAsync(request, CancellationToken.None);

        Assert.Equal("Closed", result.Status);
        Assert.Equal("closer", result.ClosedBy);
    }
}
