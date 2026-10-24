using Accounting.Domain.Enums;
using Accounting.Persistence;
using Accounting.Repositories;
using Accounting.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Accounting;

public static class DependencyInjection
{
    public static IServiceCollection AddAccountingModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing connection string 'Default'.");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<AccountNature>("account_nature");
        dataSourceBuilder.MapEnum<AccountType>("account_type");
        dataSourceBuilder.MapEnum<PeriodStatus>("period_status");
        dataSourceBuilder.MapEnum<JournalEntryStatus>("journal_entry_status");
        var dataSource = dataSourceBuilder.Build();

        services.AddSingleton(dataSource);
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dataSource, npgsqlOptions =>
        {
            npgsqlOptions.MapEnum<AccountNature>("account_nature");
            npgsqlOptions.MapEnum<AccountType>("account_type");
            npgsqlOptions.MapEnum<PeriodStatus>("period_status");
            npgsqlOptions.MapEnum<JournalEntryStatus>("journal_entry_status");
        }));

        services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
        services.AddScoped<IAccountingPeriodRepository, AccountingPeriodRepository>();
        services.AddScoped<IVoucherTypeRepository, VoucherTypeRepository>();
        services.AddScoped<IPeriodReopenRequestRepository, PeriodReopenRequestRepository>();
        services.AddScoped<IAccountBalanceSnapshotRepository, AccountBalanceSnapshotRepository>();

        services.AddScoped<PostJournalEntryService>();
        services.AddScoped<ReverseJournalEntryService>();
        services.AddScoped<CloseAccountingPeriodService>();
        services.AddScoped<RequestPeriodReopenService>();
        services.AddScoped<ResolvePeriodReopenService>();
        services.AddScoped<RecalculateAccountBalanceSnapshotService>();

        return services;
    }
}
