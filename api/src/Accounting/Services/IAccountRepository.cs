using Accounting.Domain;

namespace Accounting.Services;

public interface IAccountRepository
{
    Task<IReadOnlyList<Account>> FindManyByIdAsync(long companyId, IReadOnlyCollection<long> ids, CancellationToken cancellationToken);
}
