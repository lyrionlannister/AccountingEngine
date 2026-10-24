using Accounting.Domain;
using Accounting.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

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
    }
}
