using Accounting.Domain;
using Accounting.Domain.Enums;
using Accounting.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Accounting.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class AccountHierarchyTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public AccountHierarchyTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SetParent_ToItself_ThrowsAfterPersistence()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_postgres);
        dataSourceBuilder.MapEnum<AccountNature>("account_nature");
        dataSourceBuilder.MapEnum<AccountType>("account_type");
        await using var dataSource = dataSourceBuilder.Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(dataSource, npgsqlOptions =>
            {
                npgsqlOptions.MapEnum<AccountNature>("account_nature");
                npgsqlOptions.MapEnum<AccountType>("account_type");
            })
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.MigrateAsync();

        var account = Account.Create(1, "1000", "Cash", AccountNature.Debit, AccountType.Asset, null, true);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        Assert.NotEqual(0, account.Id);
        Assert.Throws<InvalidOperationException>(() => account.SetParent(account.Id));
    }
}
