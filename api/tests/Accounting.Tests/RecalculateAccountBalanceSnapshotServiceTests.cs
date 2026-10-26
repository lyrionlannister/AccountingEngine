using Accounting.Domain;
using Accounting.Domain.Enums;
using Accounting.DTOs;
using Accounting.Services;

namespace Accounting.Tests;

public class RecalculateAccountBalanceSnapshotServiceTests
{
    private const long CompanyId = 1;

    private static FakeAccountingPeriodRepository PeriodsWithOnePeriod()
    {
        var periods = new FakeAccountingPeriodRepository();
        periods.Periods.Add(AccountingPeriod.Create(CompanyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31)));
        return periods;
    }

    [Fact]
    public async Task RecalculateAsync_WhenNoSnapshotExists_CreatesANewOne()
    {
        var periods = PeriodsWithOnePeriod();
        var snapshots = new FakeAccountBalanceSnapshotRepository { NextSum = (300m, 300m) };
        var service = new RecalculateAccountBalanceSnapshotService(periods, snapshots, new FakeAuditTrailWriter());

        var result = await service.RecalculateAsync(
            new RecalculateAccountBalanceSnapshotRequest(CompanyId, AccountingPeriodId: 0, AccountId: 5, TriggeredBy: "tester"),
            CancellationToken.None);

        Assert.Single(snapshots.Snapshots);
        Assert.Equal(300m, result.DebitTotal);
        Assert.Equal(300m, result.CreditTotal);
    }

    [Fact]
    public async Task RecalculateAsync_WhenSnapshotExists_UpdatesItInPlace()
    {
        var periods = PeriodsWithOnePeriod();
        var snapshots = new FakeAccountBalanceSnapshotRepository { NextSum = (100m, 100m) };
        var existing = AccountBalanceSnapshot.Create(CompanyId, 0, 5, 50m, 50m, DateTimeOffset.UtcNow.AddDays(-1));
        snapshots.Snapshots.Add(existing);

        var service = new RecalculateAccountBalanceSnapshotService(periods, snapshots, new FakeAuditTrailWriter());

        var result = await service.RecalculateAsync(
            new RecalculateAccountBalanceSnapshotRequest(CompanyId, AccountingPeriodId: 0, AccountId: 5, TriggeredBy: "tester"),
            CancellationToken.None);

        Assert.Single(snapshots.Snapshots);
        Assert.Equal(100m, result.DebitTotal);
        Assert.Equal(100m, result.CreditTotal);
        Assert.Equal(100m, existing.DebitTotal);
    }

    [Fact]
    public async Task RecalculateAsync_CalledTwice_RecordsTwoSeparateAuditEntries()
    {
        var periods = PeriodsWithOnePeriod();
        var snapshots = new FakeAccountBalanceSnapshotRepository { NextSum = (100m, 100m) };
        var auditTrail = new FakeAuditTrailWriter();
        var service = new RecalculateAccountBalanceSnapshotService(periods, snapshots, auditTrail);
        var request = new RecalculateAccountBalanceSnapshotRequest(CompanyId, AccountingPeriodId: 0, AccountId: 5, TriggeredBy: "tester");

        await service.RecalculateAsync(request, CancellationToken.None);
        await service.RecalculateAsync(request, CancellationToken.None);

        Assert.Equal(2, auditTrail.Records.Count);
        Assert.All(auditTrail.Records, r => Assert.Equal(AuditOperation.AccountBalanceRecalculated, r.Operation));
    }
}
