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
}
