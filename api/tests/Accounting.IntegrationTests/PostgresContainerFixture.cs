using Npgsql;
using Testcontainers.PostgreSql;

namespace Accounting.IntegrationTests;

public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:18-alpine")
        .Build();

    public Task InitializeAsync()
    {
        // Leida por la migracion AddAppRoleAndRevokeAuditPrivileges (CREATE/ALTER ROLE
        // motor_contable_app) — sin esto, CUALQUIER prueba que migre la base falla, no solo
        // las que prueban privilegios. Fijada una vez por coleccion; paralelizacion deshabilitada
        // en este ensamblado (AssemblyInfo.cs), no hay condicion de carrera.
        Environment.SetEnvironmentVariable("ACCOUNTING_APP_DB_PASSWORD", "test-app-role-password");
        return _container.StartAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public async Task<string> CreateDatabaseAsync()
    {
        var databaseName = $"test_{Guid.NewGuid():N}";
        var adminConnectionString = _container.GetConnectionString();

        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE \"{databaseName}\";";
        await command.ExecuteNonQueryAsync();

        return new NpgsqlConnectionStringBuilder(adminConnectionString) { Database = databaseName }.ConnectionString;
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresContainerFixture>
{
    public const string Name = "Postgres container";
}
