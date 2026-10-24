using Accounting.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Accounting.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class AppDbContextMigrationTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public AppDbContextMigrationTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Migrate_AppliesAllPendingMigrations_AgainstRealPostgres()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres)
            .Options;

        await using var context = new AppDbContext(options);

        await context.Database.MigrateAsync();

        var pending = await context.Database.GetPendingMigrationsAsync();
        Assert.Empty(pending);
    }

    [Theory]
    [InlineData("accounts")]
    [InlineData("cost_centers")]
    [InlineData("parties")]
    [InlineData("voucher_types")]
    [InlineData("accounting_periods")]
    [InlineData("journal_entries")]
    [InlineData("period_reopen_requests")]
    public async Task Migrate_CreatesUniqueConstraintOnPublicId(string table)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.MigrateAsync();

        var isUnique = await IsColumnUniqueAsync(context, table, "public_id");

        Assert.True(isUnique, $"Expected a unique constraint on {table}.public_id.");
    }

    [Fact]
    public async Task Migrate_CreatesUniqueIndexOnJournalEntryIdempotencySource()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.MigrateAsync();

        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT 1
            FROM pg_index i
            JOIN pg_class c ON c.oid = i.indexrelid
            WHERE c.relname = 'ix_journal_entries_idempotency_source'
              AND i.indisunique;
            """;
        var result = await command.ExecuteScalarAsync();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Migrate_CreatesDebitXorCreditCheckConstraint_OnJournalEntryLines()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.MigrateAsync();

        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT 1
            FROM pg_constraint
            WHERE conname = 'ck_journal_entry_lines_debit_xor_credit'
              AND contype = 'c';
            """;
        var result = await command.ExecuteScalarAsync();

        Assert.NotNull(result);
    }

    private static async Task<bool> IsColumnUniqueAsync(AppDbContext context, string table, string column)
    {
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT 1
            FROM pg_index i
            JOIN pg_attribute a ON a.attrelid = i.indrelid AND a.attnum = ANY(i.indkey)
            WHERE i.indrelid = @table::regclass
              AND i.indisunique
              AND a.attname = @column
              AND array_length(i.indkey, 1) = 1;
            """;

        var tableParam = command.CreateParameter();
        tableParam.ParameterName = "table";
        tableParam.Value = table;
        command.Parameters.Add(tableParam);

        var columnParam = command.CreateParameter();
        columnParam.ParameterName = "column";
        columnParam.Value = column;
        command.Parameters.Add(columnParam);

        var result = await command.ExecuteScalarAsync();
        return result is not null;
    }
}
