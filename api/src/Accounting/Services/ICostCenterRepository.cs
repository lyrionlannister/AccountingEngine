using Accounting.Domain;

namespace Accounting.Services;

public interface ICostCenterRepository
{
    Task<IReadOnlyList<CostCenter>> FindManyByIdAsync(long companyId, IReadOnlyCollection<long> ids, CancellationToken cancellationToken);
}
