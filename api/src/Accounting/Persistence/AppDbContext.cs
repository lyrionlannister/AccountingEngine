using Accounting.Domain;
using Accounting.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace Accounting.Persistence;

public sealed class AppDbContext : DbContext
{
    private readonly ICurrentCompanyContext _currentCompany;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentCompanyContext currentCompany) : base(options)
    {
        _currentCompany = currentCompany;
    }

    // Leído por el query filter de ApplyCompanyScopeFilters a través de "this": no pasar
    // _currentCompany directamente, o el filtro queda fijo a la primera instancia del modelo cacheado.
    public long CurrentCompanyId => _currentCompany.CompanyId;

    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<Party> Parties => Set<Party>();
    public DbSet<VoucherType> VoucherTypes => Set<VoucherType>();
    public DbSet<AccountingPeriod> AccountingPeriods => Set<AccountingPeriod>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<AccountBalanceSnapshot> AccountBalanceSnapshots => Set<AccountBalanceSnapshot>();
    public DbSet<PeriodReopenRequest> PeriodReopenRequests => Set<PeriodReopenRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<AccountNature>(name: "naturaleza_cuenta");
        modelBuilder.HasPostgresEnum<AccountType>(name: "tipo_cuenta");
        modelBuilder.HasPostgresEnum<PeriodStatus>(name: "estado_periodo");
        modelBuilder.HasPostgresEnum<JournalEntryStatus>(name: "estado_asiento");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.ApplyCompanyScopeFilters(this, ctx => ctx.CurrentCompanyId, typeof(Currency), typeof(ExchangeRate));
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ChangeTracker.ApplyCompanyIdOnAdd(_currentCompany);
        return base.SaveChangesAsync(cancellationToken);
    }
}
