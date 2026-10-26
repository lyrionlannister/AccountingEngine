using Accounting.Domain.Enums;

namespace Accounting.Services;

// Un solo metodo, no uno por operacion: evita repetir la misma logica de armado de
// AuditTrailEntry en cada caso de uso. No llama SaveChangesAsync — viaja en la misma
// llamada que la mutacion de negocio que cada Service ya hace a continuacion.
public interface IAuditTrailWriter
{
    void Record(AuditOperation operation, long companyId, string actor, object auditedEntity, DateTimeOffset occurredAt, string? notes = null);
}
