using Accounting.Domain.Enums;
using Accounting.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Accounting.IntegrationTests;

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    private TestDatabase(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public static async Task<TestDatabase> CreateAsync(string connectionString)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        MapAdoEnums(dataSourceBuilder);
        var dataSource = dataSourceBuilder.Build();

        var database = new TestDatabase(dataSource);

        await using var migrationContext = database.NewContext();
        await migrationContext.Database.MigrateAsync();

        return database;
    }

    public AppDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_dataSource, MapEfEnums)
            .Options;

        return new AppDbContext(options);
    }

    public async ValueTask DisposeAsync() => await _dataSource.DisposeAsync();

    private static void MapAdoEnums(NpgsqlDataSourceBuilder builder)
    {
        builder.MapEnum<AccountNature>("naturaleza_cuenta");
        builder.MapEnum<AccountType>("tipo_cuenta");
        builder.MapEnum<PeriodStatus>("estado_periodo");
        builder.MapEnum<JournalEntryStatus>("estado_asiento");
    }

    private static void MapEfEnums(Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.NpgsqlDbContextOptionsBuilder builder)
    {
        builder.MapEnum<AccountNature>("naturaleza_cuenta");
        builder.MapEnum<AccountType>("tipo_cuenta");
        builder.MapEnum<PeriodStatus>("estado_periodo");
        builder.MapEnum<JournalEntryStatus>("estado_asiento");
    }
}
