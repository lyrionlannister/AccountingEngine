using Accounting.Domain;

namespace Accounting.Services;

public interface IPartyRepository
{
    Task<IReadOnlyList<Party>> FindManyByIdAsync(long companyId, IReadOnlyCollection<long> ids, CancellationToken cancellationToken);
}
