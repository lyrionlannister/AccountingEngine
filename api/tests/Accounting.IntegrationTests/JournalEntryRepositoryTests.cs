using Accounting.Domain;
using Accounting.Domain.Enums;
using Accounting.DTOs;
using Accounting.Persistence;
using Accounting.Repositories;
using Accounting.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Accounting.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class JournalEntryRepositoryTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public JournalEntryRepositoryTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private static AppDbContext NewContext(string connectionString, long companyId)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<AccountNature>("naturaleza_cuenta");
        dataSourceBuilder.MapEnum<AccountType>("tipo_cuenta");
        dataSourceBuilder.MapEnum<PeriodStatus>("estado_periodo");
        dataSourceBuilder.MapEnum<JournalEntryStatus>("estado_asiento");
        var dataSource = dataSourceBuilder.Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(dataSource, npgsqlOptions =>
            {
                npgsqlOptions.MapEnum<AccountNature>("naturaleza_cuenta");
                npgsqlOptions.MapEnum<AccountType>("tipo_cuenta");
                npgsqlOptions.MapEnum<PeriodStatus>("estado_periodo");
                npgsqlOptions.MapEnum<JournalEntryStatus>("estado_asiento");
            })
            .Options;

        return new AppDbContext(options, new FixedCurrentCompanyContext(companyId));
    }

    [Fact]
    public async Task FindByPublicIdAsync_WithEntryFromAnotherCompany_ReturnsNull()
    {
        const long companyId = 1;
        const long otherCompanyId = 2;

        await using var context = NewContext(_postgres, companyId);
        await context.Database.MigrateAsync();

        var currency = Currency.Create("USD", "US Dollar", 2);
        var debitAccount = Account.Create(companyId, "1000", "Cash", AccountNature.Debit, AccountType.Asset, null, true);
        var creditAccount = Account.Create(companyId, "4000", "Revenue", AccountNature.Credit, AccountType.Revenue, null, true);
        var voucherType = VoucherType.Create(companyId, "GEN", "General", VoucherNumberingMode.Automatic);
        var period = AccountingPeriod.Create(companyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        context.Currencies.Add(currency);
        context.Accounts.AddRange(debitAccount, creditAccount);
        context.VoucherTypes.Add(voucherType);
        context.AccountingPeriods.Add(period);
        await context.SaveChangesAsync();

        var journalEntries = new JournalEntryRepository(context);
        var service = new PostJournalEntryService(
            journalEntries, new AccountingPeriodRepository(context), new VoucherTypeRepository(context),
            new AccountRepository(context), new CostCenterRepository(context), new PartyRepository(context));

        var request = new PostJournalEntryRequest(
            companyId, voucherType.Id, period.Id, new DateOnly(2026, 1, 15), "Test entry", "tester",
            new List<JournalEntryLineRequest>
            {
                new(debitAccount.Id, null, null, currency.Id, 100m, 0m),
                new(creditAccount.Id, null, null, currency.Id, 0m, 100m)
            });

        var posted = await service.PostAsync(request, CancellationToken.None);

        var foundInSameCompany = await journalEntries.FindByPublicIdAsync(companyId, posted.PublicId, CancellationToken.None);
        var foundInOtherCompany = await journalEntries.FindByPublicIdAsync(otherCompanyId, posted.PublicId, CancellationToken.None);

        Assert.NotNull(foundInSameCompany);
        Assert.Equal(posted.Id, foundInSameCompany!.Id);
        Assert.Null(foundInOtherCompany);
    }
}
