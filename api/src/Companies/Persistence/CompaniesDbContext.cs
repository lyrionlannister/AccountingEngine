using Companies.Domain;
using Companies.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Companies.Persistence;

// Companies es dueño de la Membership que define el aislamiento: sus propias tablas
// no pasan por ApplyCompanyScopeFilters (evita la circularidad descrita en el plan).
public sealed class CompaniesDbContext : DbContext
{
    public CompaniesDbContext(DbContextOptions<CompaniesDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<PlatformAdministrator> PlatformAdministrators => Set<PlatformAdministrator>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<CompanyMembershipRole>(name: "rol_membresia");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CompaniesDbContext).Assembly);
    }
}
