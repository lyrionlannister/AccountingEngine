using Companies.Domain;

namespace Companies.Services;

public interface ITenantRepository
{
    Task<Tenant?> FindByIdAsync(long id, CancellationToken cancellationToken);

    // Las tres entidades se crean en una sola transacción: Company necesita el Id real
    // del Tenant y Membership el Id real de Company, ambos solo conocidos tras persistir.
    Task<(Tenant Tenant, Company Company, Membership Membership)> CreateWithFirstCompanyAsync(
        Func<Tenant> createTenant,
        Func<long, Company> createCompany,
        Func<long, Membership> createMembership,
        CancellationToken cancellationToken);
}
