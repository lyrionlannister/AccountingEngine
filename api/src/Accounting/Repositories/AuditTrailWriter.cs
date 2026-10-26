using Accounting.Domain;
using Accounting.Domain.Enums;
using Accounting.Persistence;
using Accounting.Services;

namespace Accounting.Repositories;

public sealed class AuditTrailWriter : IAuditTrailWriter
{
    private readonly AppDbContext _context;

    public AuditTrailWriter(AppDbContext context) => _context = context;

    public void Record(AuditOperation operation, long companyId, string actor, object auditedEntity, DateTimeOffset occurredAt, string? notes = null)
    {
        var entry = auditedEntity switch
        {
            JournalEntry journalEntry => AuditTrailEntry.ForJournalEntry(journalEntry, operation, companyId, actor, occurredAt, notes),
            AccountingPeriod period => AuditTrailEntry.ForAccountingPeriod(period, operation, companyId, actor, occurredAt, notes),
            PeriodReopenRequest request => AuditTrailEntry.ForPeriodReopenRequest(request, operation, companyId, actor, occurredAt, notes),
            AccountBalanceSnapshot snapshot => AuditTrailEntry.ForAccountBalanceSnapshot(snapshot, operation, companyId, actor, occurredAt, notes),
            _ => throw new ArgumentException($"Unsupported audited entity type: {auditedEntity.GetType().Name}.", nameof(auditedEntity))
        };

        _context.AuditTrailEntries.Add(entry);
    }
}
