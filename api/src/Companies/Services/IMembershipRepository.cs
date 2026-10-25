using Companies.Domain;

namespace Companies.Services;

public interface IMembershipRepository
{
    Task<Membership?> FindActiveAsync(long userId, long companyId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Membership>> ListActiveByUserAsync(long userId, CancellationToken cancellationToken);

    Task AddAsync(Membership membership, CancellationToken cancellationToken);
}
