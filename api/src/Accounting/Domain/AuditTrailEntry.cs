using Accounting.Domain.Enums;

namespace Accounting.Domain;

// Referencias por navegación (no solo FK escalar): al auditar un insert nuevo (p. ej. un
// JournalEntry recién contabilizado) su Id todavía no existe antes de SaveChangesAsync. EF Core
// solo resuelve ese FK en el mismo SaveChanges si la relación se expresa por navegación.
public sealed class AuditTrailEntry
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long CompanyId { get; private set; }
    public AuditOperation Operation { get; private set; }
    public string Actor { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }
    public string? Notes { get; private set; }

    public long? JournalEntryId { get; private set; }
    public JournalEntry? JournalEntry { get; private set; }
    public long? AccountingPeriodId { get; private set; }
    public AccountingPeriod? AccountingPeriod { get; private set; }
    public long? PeriodReopenRequestId { get; private set; }
    public PeriodReopenRequest? PeriodReopenRequest { get; private set; }
    public long? AccountBalanceSnapshotId { get; private set; }
    public AccountBalanceSnapshot? AccountBalanceSnapshot { get; private set; }

    private AuditTrailEntry()
    {
    }

    public static AuditTrailEntry ForJournalEntry(JournalEntry journalEntry, AuditOperation operation, long companyId, string actor, DateTimeOffset occurredAt, string? notes = null) =>
        Create(companyId, operation, actor, occurredAt, notes, journalEntry: journalEntry);

    public static AuditTrailEntry ForAccountingPeriod(AccountingPeriod accountingPeriod, AuditOperation operation, long companyId, string actor, DateTimeOffset occurredAt, string? notes = null) =>
        Create(companyId, operation, actor, occurredAt, notes, accountingPeriod: accountingPeriod);

    public static AuditTrailEntry ForPeriodReopenRequest(PeriodReopenRequest periodReopenRequest, AuditOperation operation, long companyId, string actor, DateTimeOffset occurredAt, string? notes = null) =>
        Create(companyId, operation, actor, occurredAt, notes, periodReopenRequest: periodReopenRequest);

    public static AuditTrailEntry ForAccountBalanceSnapshot(AccountBalanceSnapshot accountBalanceSnapshot, AuditOperation operation, long companyId, string actor, DateTimeOffset occurredAt, string? notes = null) =>
        Create(companyId, operation, actor, occurredAt, notes, accountBalanceSnapshot: accountBalanceSnapshot);

    private static AuditTrailEntry Create(
        long companyId,
        AuditOperation operation,
        string actor,
        DateTimeOffset occurredAt,
        string? notes,
        JournalEntry? journalEntry = null,
        AccountingPeriod? accountingPeriod = null,
        PeriodReopenRequest? periodReopenRequest = null,
        AccountBalanceSnapshot? accountBalanceSnapshot = null)
    {
        if (string.IsNullOrWhiteSpace(actor))
            throw new ArgumentException("Actor is required.", nameof(actor));

        var referenceCount = new object?[] { journalEntry, accountingPeriod, periodReopenRequest, accountBalanceSnapshot }
            .Count(reference => reference is not null);
        if (referenceCount != 1)
            throw new ArgumentException("Exactly one audited entity reference must be set.");

        return new AuditTrailEntry
        {
            CompanyId = companyId,
            Operation = operation,
            Actor = actor,
            OccurredAt = occurredAt,
            Notes = notes,
            JournalEntry = journalEntry,
            AccountingPeriod = accountingPeriod,
            PeriodReopenRequest = periodReopenRequest,
            AccountBalanceSnapshot = accountBalanceSnapshot
        };
    }
}
