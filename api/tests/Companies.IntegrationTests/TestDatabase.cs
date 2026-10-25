using Companies.Domain.Enums;
using Companies.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Companies.IntegrationTests;

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    private TestDatabase(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public static async Task<TestDatabase> CreateAsync(string connectionString)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<CompanyMembershipRole>("rol_membresia");
        var dataSource = dataSourceBuilder.Build();

        var database = new TestDatabase(dataSource);

        await using var migrationContext = database.NewContext();
        await migrationContext.Database.MigrateAsync();

        return database;
    }

    public CompaniesDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<CompaniesDbContext>()
            .UseNpgsql(_dataSource, builder => builder.MapEnum<CompanyMembershipRole>("rol_membresia"))
            .Options;

        return new CompaniesDbContext(options);
    }

    public async ValueTask DisposeAsync() => await _dataSource.DisposeAsync();
}
