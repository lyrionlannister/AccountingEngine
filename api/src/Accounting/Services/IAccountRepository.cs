using Accounting.Domain;

namespace Accounting.Services;

public interface IAccountRepository
{
    Task<IReadOnlyList<Account>> FindManyByIdAsync(long companyId, IReadOnlyCollection<long> ids, CancellationToken cancellationToken);

    Task<Account?> FindByPublicIdAsync(long companyId, Guid publicId, CancellationToken cancellationToken);
}
