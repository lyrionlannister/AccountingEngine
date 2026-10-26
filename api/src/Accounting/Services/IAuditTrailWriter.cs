using Accounting.Domain.Enums;

namespace Accounting.Services;

public interface IAuditTrailWriter
{
    void Record(AuditOperation operation, long companyId, string actor, object auditedEntity, DateTimeOffset occurredAt, string? notes = null);
}
