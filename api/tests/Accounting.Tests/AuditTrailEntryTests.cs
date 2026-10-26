using Accounting.Domain;
using Accounting.Domain.Enums;

namespace Accounting.Tests;

public class AuditTrailEntryTests
{
    [Fact]
    public void Create_ForJournalEntryPosted_SetsOnlyJournalEntryReference()
    {
        var entry = AuditTrailEntry.Create(1, AuditOperation.JournalEntryPosted, "user-1", DateTimeOffset.UtcNow,
            journalEntryId: 42, accountingPeriodId: null, periodReopenRequestId: null, accountBalanceSnapshotId: null);

        Assert.Equal(42, entry.JournalEntryId);
        Assert.Null(entry.AccountingPeriodId);
        Assert.Null(entry.PeriodReopenRequestId);
        Assert.Null(entry.AccountBalanceSnapshotId);
        Assert.Equal(AuditOperation.JournalEntryPosted, entry.Operation);
        Assert.Equal("user-1", entry.Actor);
    }

    [Fact]
    public void Create_WithNoReference_Throws()
    {
        Assert.Throws<ArgumentException>(() => AuditTrailEntry.Create(1, AuditOperation.PeriodClosed, "user-1", DateTimeOffset.UtcNow,
            journalEntryId: null, accountingPeriodId: null, periodReopenRequestId: null, accountBalanceSnapshotId: null));
    }

    [Fact]
    public void Create_WithTwoReferences_Throws()
    {
        Assert.Throws<ArgumentException>(() => AuditTrailEntry.Create(1, AuditOperation.PeriodClosed, "user-1", DateTimeOffset.UtcNow,
            journalEntryId: 1, accountingPeriodId: 2, periodReopenRequestId: null, accountBalanceSnapshotId: null));
    }

    [Fact]
    public void Create_WithoutActor_Throws()
    {
        Assert.Throws<ArgumentException>(() => AuditTrailEntry.Create(1, AuditOperation.PeriodClosed, " ", DateTimeOffset.UtcNow,
            journalEntryId: null, accountingPeriodId: 2, periodReopenRequestId: null, accountBalanceSnapshotId: null));
    }
}
