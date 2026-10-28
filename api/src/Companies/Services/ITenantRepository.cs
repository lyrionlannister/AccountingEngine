using System.Data.Common;
using Companies.Domain;

namespace Companies.Services;

public interface ITenantRepository
{
    Task<Tenant?> FindByIdAsync(long id, CancellationToken cancellationToken);

    Task<(Tenant Tenant, Company Company, Membership Membership)> CreateWithFirstCompanyAsync(
        Func<Tenant> createTenant,
        Func<long, Company> createCompany,
        Func<long, Membership> createMembership,
        CancellationToken cancellationToken);

    Task<(long OwnerUserId, Guid OwnerPublicId, Tenant Tenant, Company Company, Membership Membership)> CreateWithOwnerAsync(
        Func<DbConnection, DbTransaction, CancellationToken, Task<(long UserId, Guid UserPublicId)>> createOwner,
        Func<Tenant> createTenant,
        Func<long, Company> createCompany,
        Func<long, Membership> createMembership,
        CancellationToken cancellationToken);
}
