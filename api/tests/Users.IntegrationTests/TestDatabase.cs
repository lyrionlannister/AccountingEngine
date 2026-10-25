using Companies.Domain.Enums;
using Companies.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Users.Persistence;

namespace Users.IntegrationTests;

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    private TestDatabase(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    // Reproduce el orden real de despliegue: las migraciones de Companies (que crean la tabla
    // "usuarios" e históricamente le agregaron sus FKs) corrieron antes de que Users existiera.
    // InitialUsersSchema es un no-op a propósito — ver tasks/plan-users.md, "traspaso de ownership".
    public static async Task<TestDatabase> CreateAsync(string connectionString)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<CompanyMembershipRole>("rol_membresia");
        var dataSource = dataSourceBuilder.Build();

        var database = new TestDatabase(dataSource);

        var companiesOptions = new DbContextOptionsBuilder<CompaniesDbContext>()
            .UseNpgsql(dataSource, b => b.MapEnum<CompanyMembershipRole>("rol_membresia"))
            .Options;
        await using var companiesContext = new CompaniesDbContext(companiesOptions);
        await companiesContext.Database.MigrateAsync();

        await using var usersContext = database.NewContext();
        await usersContext.Database.MigrateAsync();

        return database;
    }

    public UsersDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseNpgsql(_dataSource)
            .Options;

        return new UsersDbContext(options);
    }

    public async ValueTask DisposeAsync() => await _dataSource.DisposeAsync();
}
