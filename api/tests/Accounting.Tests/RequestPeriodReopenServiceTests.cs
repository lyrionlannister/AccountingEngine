using Accounting.Domain;
using Accounting.Domain.Enums;
using Accounting.DTOs;
using Accounting.Services;

namespace Accounting.Tests;

public class RequestPeriodReopenServiceTests
{
    private const long CompanyId = 1;

    [Fact]
    public async Task RequestAsync_OnSuccess_RecordsPeriodReopenRequestedInAuditTrail()
    {
        var periods = new FakeAccountingPeriodRepository();
        var period = AccountingPeriod.Create(CompanyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        period.Close("closer", DateTimeOffset.UtcNow);
        periods.Periods.Add(period);

        var reopenRequests = new FakePeriodReopenRequestRepository();
        var auditTrail = new FakeAuditTrailWriter();
        var service = new RequestPeriodReopenService(periods, reopenRequests, auditTrail);

        await service.RequestAsync(
            new RequestPeriodReopenRequest(CompanyId, AccountingPeriodId: 0, "Corrección", "requester-1"),
            CancellationToken.None);

        var record = Assert.Single(auditTrail.Records);
        Assert.Equal(AuditOperation.PeriodReopenRequested, record.Operation);
        Assert.Equal("requester-1", record.Actor);
        Assert.IsType<PeriodReopenRequest>(record.AuditedEntity);
    }
}
