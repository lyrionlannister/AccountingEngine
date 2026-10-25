using Accounting.Domain;

namespace Accounting.Services;

public interface IPeriodReopenRequestRepository
{
    Task AddAsync(PeriodReopenRequest request, CancellationToken cancellationToken);

    Task<PeriodReopenRequest?> FindByIdAsync(long companyId, long id, CancellationToken cancellationToken);

    Task<PeriodReopenRequest?> FindByPublicIdAsync(long companyId, Guid publicId, CancellationToken cancellationToken);

    Task UpdateAsync(PeriodReopenRequest request, CancellationToken cancellationToken);
}
