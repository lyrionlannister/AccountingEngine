using Companies.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Companies.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class SchemaConstraintTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public SchemaConstraintTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Migrate_AppliesAllPendingMigrations_AgainstRealPostgres()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        var pending = await context.Database.GetPendingMigrationsAsync();

        Assert.Empty(pending);
    }

    [Fact]
    public async Task Memberships_RejectsDuplicateUserCompanyPair()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        await SeedTenantCompanyAndUserAsync(context);

        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO membresias (usuario_id, empresa_id, rol, activo, otorgado_en)
            VALUES ((SELECT id FROM usuarios LIMIT 1), (SELECT id FROM empresas LIMIT 1), 'admin', true, now());
            """);

        var ex = await Record.ExceptionAsync(() => context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO membresias (usuario_id, empresa_id, rol, activo, otorgado_en)
            VALUES ((SELECT id FROM usuarios LIMIT 1), (SELECT id FROM empresas LIMIT 1), 'viewer', true, now());
            """));

        Assert.IsType<PostgresException>(ex?.InnerException ?? ex);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, ((PostgresException)(ex!.InnerException ?? ex)).SqlState);
    }

    [Fact]
    public async Task Users_RejectsDuplicateEmailDifferingOnlyByCase()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO usuarios (correo, contrasena_hash, activo) VALUES ('Someone@Example.com', 'hash', true);");

        var ex = await Record.ExceptionAsync(() => context.Database.ExecuteSqlRawAsync(
            "INSERT INTO usuarios (correo, contrasena_hash, activo) VALUES ('someone@EXAMPLE.com', 'hash', true);"));

        Assert.IsType<PostgresException>(ex?.InnerException ?? ex);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, ((PostgresException)(ex!.InnerException ?? ex)).SqlState);
    }

    private static async Task SeedTenantCompanyAndUserAsync(CompaniesDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO inquilinos (nombre) VALUES ('Acme Holdings');");

        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO empresas (inquilino_id, nombre, codigo_pais, activo)
            VALUES ((SELECT id FROM inquilinos LIMIT 1), 'Acme Colombia', 'CO', true);
            """);

        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO usuarios (correo, contrasena_hash, activo) VALUES ('admin@acme.test', 'hash', true);");
    }
}
