using Accounting.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Accounting.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class SchemaConstraintTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public SchemaConstraintTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<AppDbContext> MigratedContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres)
            .Options;

        var context = new AppDbContext(options, new FixedCurrentCompanyContext(1));
        await context.Database.MigrateAsync();
        return context;
    }

    [Fact]
    public async Task ExchangeRates_RejectsDuplicateRateForSamePairAndDate()
    {
        await using var context = await MigratedContextAsync();

        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO monedas (codigo, nombre, decimales, activo) VALUES ('USD', 'US Dollar', 2, true), ('MXN', 'Mexican Peso', 2, true);");

        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO tipos_cambio (moneda_origen_id, moneda_destino_id, tasa, fecha_vigencia, fuente)
            VALUES ((SELECT id FROM monedas WHERE codigo = 'USD'), (SELECT id FROM monedas WHERE codigo = 'MXN'), 18.50, '2026-01-15', 'test');
            """);

        var ex = await Record.ExceptionAsync(() => context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO tipos_cambio (moneda_origen_id, moneda_destino_id, tasa, fecha_vigencia, fuente)
            VALUES ((SELECT id FROM monedas WHERE codigo = 'USD'), (SELECT id FROM monedas WHERE codigo = 'MXN'), 18.70, '2026-01-15', 'test');
            """));

        Assert.IsType<PostgresException>(ex?.InnerException ?? ex);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, ((PostgresException)(ex!.InnerException ?? ex)).SqlState);
    }

    [Fact]
    public async Task JournalEntryLines_RejectsZeroDebitAndZeroCredit()
    {
        await using var context = await MigratedContextAsync();

        await SeedLineParentRowsAsync(context);

        var ex = await Record.ExceptionAsync(() => context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO lineas_asiento (asiento_contable_id, numero_linea, cuenta_id, moneda_id, monto_debito, monto_credito)
            VALUES ((SELECT id FROM asientos_contables LIMIT 1), 1, (SELECT id FROM cuentas LIMIT 1), (SELECT id FROM monedas LIMIT 1), 0, 0);
            """));

        Assert.IsType<PostgresException>(ex?.InnerException ?? ex);
        Assert.Equal(PostgresErrorCodes.CheckViolation, ((PostgresException)(ex!.InnerException ?? ex)).SqlState);
    }

    [Fact]
    public async Task OpeningBalances_RejectsZeroDebitAndZeroCredit()
    {
        await using var context = await MigratedContextAsync();

        await SeedLineParentRowsAsync(context);

        var ex = await Record.ExceptionAsync(() => context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO saldos_iniciales (periodo_contable_id, cuenta_id, moneda_id, monto_debito, monto_credito)
            VALUES ((SELECT id FROM periodos_contables LIMIT 1), (SELECT id FROM cuentas LIMIT 1), (SELECT id FROM monedas LIMIT 1), 0, 0);
            """));

        Assert.IsType<PostgresException>(ex?.InnerException ?? ex);
        Assert.Equal(PostgresErrorCodes.CheckViolation, ((PostgresException)(ex!.InnerException ?? ex)).SqlState);
    }

    [Theory]
    [InlineData("ix_tipos_cambio_par_fecha")]
    [InlineData("ix_asientos_contables_empresa_periodo_estado")]
    [InlineData("ix_solicitudes_reapertura_periodo_empresa_periodo_estado")]
    public async Task Migrate_UsesExplicitShortIndexName_NotTruncatedByPostgres(string expectedIndexName)
    {
        await using var context = await MigratedContextAsync();

        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM pg_class WHERE relname = @name;";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "name";
        parameter.Value = expectedIndexName;
        command.Parameters.Add(parameter);

        var result = await command.ExecuteScalarAsync();

        Assert.NotNull(result);
    }

    private static async Task SeedLineParentRowsAsync(AppDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO monedas (codigo, nombre, decimales, activo) VALUES ('USD', 'US Dollar', 2, true);");

        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO cuentas (empresa_id, codigo, nombre, naturaleza, tipo_cuenta, permite_movimientos, requiere_centro_costo, requiere_tercero, activo)
            VALUES (1, '1000', 'Cash', 'debit', 'asset', true, false, false, true);
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO periodos_contables (empresa_id, nombre, fecha_inicio, fecha_fin, estado)
            VALUES (1, '2026-01', '2026-01-01', '2026-01-31', 'open');
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO tipos_comprobante (empresa_id, codigo, nombre, modo_numeracion, activo)
            VALUES (1, 'GEN', 'General', 'Automatic', true);
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO asientos_contables (empresa_id, tipo_comprobante_id, periodo_contable_id, estado, fecha_asiento, descripcion)
            VALUES (1, (SELECT id FROM tipos_comprobante LIMIT 1), (SELECT id FROM periodos_contables LIMIT 1), 'draft', '2026-01-15', 'seed');
            """);
    }
}
