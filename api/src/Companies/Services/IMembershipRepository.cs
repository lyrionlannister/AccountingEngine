using Companies.Domain;

namespace Companies.Services;

public interface IMembershipRepository
{
    Task<Membership?> FindActiveAsync(long userId, long companyId, CancellationToken cancellationToken);

    Task AddAsync(Membership membership, CancellationToken cancellationToken);
}
