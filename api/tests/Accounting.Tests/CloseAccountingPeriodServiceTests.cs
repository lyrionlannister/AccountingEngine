using Accounting.Domain;
using Accounting.Domain.Enums;
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

        var service = new CloseAccountingPeriodService(periods, new FakeJournalEntryRepository(), new FakeAuditTrailWriter());
        var request = new CloseAccountingPeriodRequest(CompanyId, AccountingPeriodId: 0, ClosedBy: "second-closer");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CloseAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CloseAsync_WhenOpen_ClosesThePeriod()
    {
        var periods = new FakeAccountingPeriodRepository();
        var period = AccountingPeriod.Create(CompanyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        periods.Periods.Add(period);

        var service = new CloseAccountingPeriodService(periods, new FakeJournalEntryRepository(), new FakeAuditTrailWriter());
        var request = new CloseAccountingPeriodRequest(CompanyId, AccountingPeriodId: 0, ClosedBy: "closer");

        var result = await service.CloseAsync(request, CancellationToken.None);

        Assert.Equal("Closed", result.Status);
        Assert.Equal("closer", result.ClosedBy);
    }

    [Fact]
    public async Task CloseAsync_WithDraftEntriesInThePeriod_ThrowsAndDoesNotClose()
    {
        var periods = new FakeAccountingPeriodRepository();
        var period = AccountingPeriod.Create(CompanyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        periods.Periods.Add(period);

        var journalEntries = new FakeJournalEntryRepository();
        journalEntries.Entries.Add(JournalEntry.Create(
            CompanyId, voucherTypeId: 1, accountingPeriodId: 0, new DateOnly(2026, 1, 15), "draft entry"));

        var service = new CloseAccountingPeriodService(periods, journalEntries, new FakeAuditTrailWriter());
        var request = new CloseAccountingPeriodRequest(CompanyId, AccountingPeriodId: 0, ClosedBy: "closer");

        await Assert.ThrowsAsync<AccountingPeriodHasDraftEntriesException>(() => service.CloseAsync(request, CancellationToken.None));
        Assert.Equal(PeriodStatus.Open, period.Status);
    }

    [Fact]
    public async Task CloseAsync_OnSuccess_RecordsPeriodClosedInAuditTrail()
    {
        var periods = new FakeAccountingPeriodRepository();
        var period = AccountingPeriod.Create(CompanyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        periods.Periods.Add(period);
        var auditTrail = new FakeAuditTrailWriter();

        var service = new CloseAccountingPeriodService(periods, new FakeJournalEntryRepository(), auditTrail);
        var request = new CloseAccountingPeriodRequest(CompanyId, AccountingPeriodId: 0, ClosedBy: "closer");

        await service.CloseAsync(request, CancellationToken.None);

        var record = Assert.Single(auditTrail.Records);
        Assert.Equal(AuditOperation.PeriodClosed, record.Operation);
        Assert.Equal("closer", record.Actor);
        Assert.Same(period, record.AuditedEntity);
    }
}
