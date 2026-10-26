using Accounting.Domain.Enums;

namespace Accounting.Domain;

public sealed class AuditTrailEntry
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long CompanyId { get; private set; }
    public AuditOperation Operation { get; private set; }
    public string Actor { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }
    public long? JournalEntryId { get; private set; }
    public long? AccountingPeriodId { get; private set; }
    public long? PeriodReopenRequestId { get; private set; }
    public long? AccountBalanceSnapshotId { get; private set; }
    public string? Notes { get; private set; }

    private AuditTrailEntry()
    {
    }

    public static AuditTrailEntry Create(
        long companyId,
        AuditOperation operation,
        string actor,
        DateTimeOffset occurredAt,
        long? journalEntryId,
        long? accountingPeriodId,
        long? periodReopenRequestId,
        long? accountBalanceSnapshotId,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(actor))
            throw new ArgumentException("Actor is required.", nameof(actor));

        var referenceCount = new[] { journalEntryId, accountingPeriodId, periodReopenRequestId, accountBalanceSnapshotId }
            .Count(id => id is not null);
        if (referenceCount != 1)
            throw new ArgumentException("Exactly one audited entity reference must be set.");

        return new AuditTrailEntry
        {
            CompanyId = companyId,
            Operation = operation,
            Actor = actor,
            OccurredAt = occurredAt,
            JournalEntryId = journalEntryId,
            AccountingPeriodId = accountingPeriodId,
            PeriodReopenRequestId = periodReopenRequestId,
            AccountBalanceSnapshotId = accountBalanceSnapshotId,
            Notes = notes
        };
    }
}
