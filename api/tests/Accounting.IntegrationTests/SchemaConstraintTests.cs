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

        var context = new AppDbContext(options);
        await context.Database.MigrateAsync();
        return context;
    }

    [Fact]
    public async Task ExchangeRates_RejectsDuplicateRateForSamePairAndDate()
    {
        await using var context = await MigratedContextAsync();

        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO currencies (code, name, decimal_places, is_active) VALUES ('USD', 'US Dollar', 2, true), ('MXN', 'Mexican Peso', 2, true);");

        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO exchange_rates (from_currency_id, to_currency_id, rate, effective_date, source)
            VALUES ((SELECT id FROM currencies WHERE code = 'USD'), (SELECT id FROM currencies WHERE code = 'MXN'), 18.50, '2026-01-15', 'test');
            """);

        var ex = await Record.ExceptionAsync(() => context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO exchange_rates (from_currency_id, to_currency_id, rate, effective_date, source)
            VALUES ((SELECT id FROM currencies WHERE code = 'USD'), (SELECT id FROM currencies WHERE code = 'MXN'), 18.70, '2026-01-15', 'test');
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
            INSERT INTO journal_entry_lines (journal_entry_id, line_number, account_id, currency_id, debit_amount, credit_amount)
            VALUES ((SELECT id FROM journal_entries LIMIT 1), 1, (SELECT id FROM accounts LIMIT 1), (SELECT id FROM currencies LIMIT 1), 0, 0);
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
            INSERT INTO opening_balances (accounting_period_id, account_id, currency_id, debit_amount, credit_amount)
            VALUES ((SELECT id FROM accounting_periods LIMIT 1), (SELECT id FROM accounts LIMIT 1), (SELECT id FROM currencies LIMIT 1), 0, 0);
            """));

        Assert.IsType<PostgresException>(ex?.InnerException ?? ex);
        Assert.Equal(PostgresErrorCodes.CheckViolation, ((PostgresException)(ex!.InnerException ?? ex)).SqlState);
    }

    [Theory]
    [InlineData("ix_exchange_rates_pair_date")]
    [InlineData("ix_journal_entries_company_period_status")]
    [InlineData("ix_period_reopen_requests_company_period_status")]
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
            "INSERT INTO currencies (code, name, decimal_places, is_active) VALUES ('USD', 'US Dollar', 2, true);");

        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO accounts (company_id, code, name, nature, account_type, allows_posting, requires_cost_center, requires_party, is_active)
            VALUES (1, '1000', 'Cash', 'debit', 'asset', true, false, false, true);
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO accounting_periods (company_id, name, start_date, end_date, period_status)
            VALUES (1, '2026-01', '2026-01-01', '2026-01-31', 'open');
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO voucher_types (company_id, code, name, numbering_mode, is_active)
            VALUES (1, 'GEN', 'General', 'Automatic', true);
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO journal_entries (company_id, voucher_type_id, accounting_period_id, journal_entry_status, entry_date, description)
            VALUES (1, (SELECT id FROM voucher_types LIMIT 1), (SELECT id FROM accounting_periods LIMIT 1), 'draft', '2026-01-15', 'seed');
            """);
    }
}
