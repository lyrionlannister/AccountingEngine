using Companies.Domain.Enums;
using Companies.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Users.Persistence;

namespace Api.IntegrationTests;

public sealed class ApiTestEnvironment : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine")
        .WithCommand("redis-server", "--notify-keyspace-events", "Ex")
        .Build();

    public string PostgresConnectionString => _postgres.GetConnectionString();

    public string RedisConnectionString => _redis.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(PostgresConnectionString);
        dataSourceBuilder.MapEnum<CompanyMembershipRole>("rol_membresia");
        var dataSource = dataSourceBuilder.Build();

        await using var companiesContext = new CompaniesDbContext(new DbContextOptionsBuilder<CompaniesDbContext>()
            .UseNpgsql(dataSource, b => b.MapEnum<CompanyMembershipRole>("rol_membresia"))
            .Options);
        await companiesContext.Database.MigrateAsync();

        await using var usersContext = new UsersDbContext(
            new DbContextOptionsBuilder<UsersDbContext>().UseNpgsql(PostgresConnectionString).Options);
        await usersContext.Database.MigrateAsync();

        await using var authContext = new Auth.Persistence.AuthDbContext(
            new DbContextOptionsBuilder<Auth.Persistence.AuthDbContext>().UseNpgsql(PostgresConnectionString).Options);
        await authContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync() =>
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _redis.DisposeAsync().AsTask());
}

[CollectionDefinition(Name)]
public sealed class ApiTestCollection : ICollectionFixture<ApiTestEnvironment>
{
    public const string Name = "Api test environment";
}
