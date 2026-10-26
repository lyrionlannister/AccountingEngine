using Npgsql;

namespace Accounting.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class AuditTrailPrivilegeTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public AuditTrailPrivilegeTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private NpgsqlConnection OpenAsAppRole()
    {
        var builder = new NpgsqlConnectionStringBuilder(_postgres)
        {
            Username = "motor_contable_app",
            Password = "test-app-role-password"
        };
        return new NpgsqlConnection(builder.ConnectionString);
    }

    [Fact]
    public async Task AppRole_CanSelectAndInsert_ButNotUpdateOrDelete_OnAuditoriaContable()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);

        await using var appConnection = OpenAsAppRole();
        await appConnection.OpenAsync();

        // SELECT: debe funcionar (tabla vacia, pero el permiso en si no debe fallar).
        await using (var select = appConnection.CreateCommand())
        {
            select.CommandText = "SELECT count(*) FROM auditoria_contable";
            await select.ExecuteScalarAsync();
        }

        // INSERT: le falta un empresa_id/operacion/actor/ocurrido_en validos y una referencia —
        // basta con que el error NO sea de permisos (42501) para confirmar que INSERT esta
        // concedido; se usa un INSERT sintacticamente invalido a proposito (faltan NOT NULL) para
        // no tener que sembrar un asiento real solo para esta prueba de privilegios.
        var insertEx = await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            await using var insert = appConnection.CreateCommand();
            insert.CommandText = "INSERT INTO auditoria_contable DEFAULT VALUES";
            await insert.ExecuteNonQueryAsync();
        });
        Assert.NotEqual("42501", insertEx.SqlState);

        // UPDATE/DELETE: deben fallar por permisos (InsufficientPrivilege), no por una regla de
        // la aplicacion — es la garantia real de esta fase.
        var updateEx = await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            await using var update = appConnection.CreateCommand();
            update.CommandText = "UPDATE auditoria_contable SET notas = 'x' WHERE id = 1";
            await update.ExecuteNonQueryAsync();
        });
        Assert.Equal("42501", updateEx.SqlState);

        var deleteEx = await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            await using var delete = appConnection.CreateCommand();
            delete.CommandText = "DELETE FROM auditoria_contable WHERE id = 1";
            await delete.ExecuteNonQueryAsync();
        });
        Assert.Equal("42501", deleteEx.SqlState);
    }
}
