using Companies.Domain;
using Companies.Persistence;
using Companies.Services;
using Microsoft.EntityFrameworkCore;

namespace Companies.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly CompaniesDbContext _context;

    public TenantRepository(CompaniesDbContext context) => _context = context;

    public Task<Tenant?> FindByIdAsync(long id, CancellationToken cancellationToken) =>
        _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<(Tenant Tenant, Company Company, Membership Membership)> CreateWithFirstCompanyAsync(
        Func<Tenant> createTenant,
        Func<long, Company> createCompany,
        Func<long, Membership> createMembership,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var tenant = createTenant();
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);

        var company = createCompany(tenant.Id);
        _context.Companies.Add(company);
        await _context.SaveChangesAsync(cancellationToken);

        var membership = createMembership(company.Id);
        _context.Memberships.Add(membership);
        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return (tenant, company, membership);
    }
}
