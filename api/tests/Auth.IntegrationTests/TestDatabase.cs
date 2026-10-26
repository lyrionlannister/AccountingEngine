using Auth.Persistence;
using Companies.Domain.Enums;
using Companies.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Users.Persistence;

namespace Auth.IntegrationTests;

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

        await using var companiesContext = database.NewCompaniesContext();
        await companiesContext.Database.MigrateAsync();

        await using var usersContext = database.NewUsersContext();
        await usersContext.Database.MigrateAsync();

        await using var authContext = database.NewAuthContext();
        await authContext.Database.MigrateAsync();

        return database;
    }

    public CompaniesDbContext NewCompaniesContext() =>
        new(new DbContextOptionsBuilder<CompaniesDbContext>()
            .UseNpgsql(_dataSource, b => b.MapEnum<CompanyMembershipRole>("rol_membresia"))
            .Options);

    public UsersDbContext NewUsersContext() =>
        new(new DbContextOptionsBuilder<UsersDbContext>().UseNpgsql(_dataSource).Options);

    public AuthDbContext NewAuthContext() =>
        new(new DbContextOptionsBuilder<AuthDbContext>().UseNpgsql(_dataSource).Options);

    public async ValueTask DisposeAsync() => await _dataSource.DisposeAsync();
}
