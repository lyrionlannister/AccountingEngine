using Accounting.Domain;

namespace Accounting.Services;

public interface IPeriodReopenRequestRepository
{
    Task AddAsync(PeriodReopenRequest request, CancellationToken cancellationToken);

    Task<PeriodReopenRequest?> FindByIdAsync(long companyId, long id, CancellationToken cancellationToken);

    Task<PeriodReopenRequest?> FindByPublicIdAsync(long companyId, Guid publicId, CancellationToken cancellationToken);

    Task UpdateAsync(PeriodReopenRequest request, CancellationToken cancellationToken);

    // Un solo SaveChangesAsync envuelto en transaccion explicita para TODO lo que
    // ResolvePeriodReopenService dejo trackeado (la solicitud, el periodo reabierto si aplica,
    // y la fila de auditoria) — antes eran 2 SaveChangesAsync separados, no atomicos.
    Task SaveResolutionAsync(CancellationToken cancellationToken);
}
