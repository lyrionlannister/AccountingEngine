using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Auth.IntegrationTests;

public sealed class AuthTestEnvironment : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:18-alpine")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    public async Task InitializeAsync() =>
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());

    public async Task DisposeAsync() =>
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _redis.DisposeAsync().AsTask());

    public async Task<string> CreatePostgresDatabaseAsync()
    {
        var databaseName = $"test_{Guid.NewGuid():N}";
        var adminConnectionString = _postgres.GetConnectionString();

        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE \"{databaseName}\";";
        await command.ExecuteNonQueryAsync();

        return new NpgsqlConnectionStringBuilder(adminConnectionString) { Database = databaseName }.ConnectionString;
    }

    public string RedisConnectionString => _redis.GetConnectionString();

    public Task StopRedisAsync() => _redis.StopAsync();

    public Task StartRedisAsync() => _redis.StartAsync();
}

[CollectionDefinition(Name)]
public sealed class AuthTestCollection : ICollectionFixture<AuthTestEnvironment>
{
    public const string Name = "Auth test environment";
}
