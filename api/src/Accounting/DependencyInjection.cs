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
        dataSourceBuilder.MapEnum<AccountNature>("naturaleza_cuenta");
        dataSourceBuilder.MapEnum<AccountType>("tipo_cuenta");
        dataSourceBuilder.MapEnum<PeriodStatus>("estado_periodo");
        dataSourceBuilder.MapEnum<JournalEntryStatus>("estado_asiento");
        var dataSource = dataSourceBuilder.Build();

        services.AddSingleton(dataSource);
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dataSource, npgsqlOptions =>
        {
            npgsqlOptions.MapEnum<AccountNature>("naturaleza_cuenta");
            npgsqlOptions.MapEnum<AccountType>("tipo_cuenta");
            npgsqlOptions.MapEnum<PeriodStatus>("estado_periodo");
            npgsqlOptions.MapEnum<JournalEntryStatus>("estado_asiento");
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
