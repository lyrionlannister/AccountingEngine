using Accounting.Domain;

namespace Accounting.Services;

public interface ICostCenterRepository
{
    Task<IReadOnlyList<CostCenter>> FindManyByIdAsync(long companyId, IReadOnlyCollection<long> ids, CancellationToken cancellationToken);

    Task<CostCenter?> FindByPublicIdAsync(long companyId, Guid publicId, CancellationToken cancellationToken);

    Task<IReadOnlyList<CostCenter>> FindManyByPublicIdAsync(long companyId, IReadOnlyCollection<Guid> publicIds, CancellationToken cancellationToken);
}
