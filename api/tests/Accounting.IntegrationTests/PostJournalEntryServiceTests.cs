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
public sealed class PostJournalEntryServiceTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public PostJournalEntryServiceTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PostAsync_RetriedWithNullSourceModule_ReturnsSameEntry_DoesNotDuplicate()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_postgres);
        dataSourceBuilder.MapEnum<AccountNature>("naturaleza_cuenta");
        dataSourceBuilder.MapEnum<AccountType>("tipo_cuenta");
        dataSourceBuilder.MapEnum<PeriodStatus>("estado_periodo");
        dataSourceBuilder.MapEnum<JournalEntryStatus>("estado_asiento");
        await using var dataSource = dataSourceBuilder.Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(dataSource, npgsqlOptions =>
            {
                npgsqlOptions.MapEnum<AccountNature>("naturaleza_cuenta");
                npgsqlOptions.MapEnum<AccountType>("tipo_cuenta");
                npgsqlOptions.MapEnum<PeriodStatus>("estado_periodo");
                npgsqlOptions.MapEnum<JournalEntryStatus>("estado_asiento");
            })
            .Options;

        await using var context = new AppDbContext(options, new FixedCurrentCompanyContext(1));
        await context.Database.MigrateAsync();

        const long companyId = 1;

        var currency = Currency.Create("USD", "US Dollar", 2);
        context.Currencies.Add(currency);

        var debitAccount = Account.Create(companyId, "1000", "Cash", AccountNature.Debit, AccountType.Asset, null, true);
        var creditAccount = Account.Create(companyId, "4000", "Revenue", AccountNature.Credit, AccountType.Revenue, null, true);
        context.Accounts.Add(debitAccount);
        context.Accounts.Add(creditAccount);

        var voucherType = VoucherType.Create(companyId, "GEN", "General", VoucherNumberingMode.Automatic);
        context.VoucherTypes.Add(voucherType);

        var period = AccountingPeriod.Create(companyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        context.AccountingPeriods.Add(period);

        await context.SaveChangesAsync();

        var journalEntries = new JournalEntryRepository(context);
        var periods = new AccountingPeriodRepository(context);
        var voucherTypes = new VoucherTypeRepository(context);
        var service = new PostJournalEntryService(
            journalEntries, periods, voucherTypes,
            new AccountRepository(context), new CostCenterRepository(context), new PartyRepository(context), new AuditTrailWriter(context),
            new FixedCurrentCompanyContext(companyId));

        var request = new PostJournalEntryRequest(
            companyId,
            voucherType.Id,
            period.Id,
            new DateOnly(2026, 1, 15),
            "Retry test",
            "tester",
            new List<JournalEntryLineRequest>
            {
                new(debitAccount.Id, null, null, currency.Id, 100m, 0m),
                new(creditAccount.Id, null, null, currency.Id, 0m, 100m)
            },
            SourceModule: null,
            SourceReference: "RETRY-1");

        var first = await service.PostAsync(request, CancellationToken.None);
        var second = await service.PostAsync(request, CancellationToken.None);

        Assert.Equal(first.Id, second.Id);

        var count = await context.JournalEntries.CountAsync(e => e.CompanyId == companyId && e.SourceReference == "RETRY-1");
        Assert.Equal(1, count);

        var auditCount = await context.AuditTrailEntries.CountAsync(a => a.CompanyId == companyId);
        Assert.Equal(1, auditCount);
    }

    [Fact]
    public async Task PostAsync_WithManualNumberingAndDuplicateSequence_ThrowsTypedConflict()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_postgres);
        dataSourceBuilder.MapEnum<AccountNature>("naturaleza_cuenta");
        dataSourceBuilder.MapEnum<AccountType>("tipo_cuenta");
        dataSourceBuilder.MapEnum<PeriodStatus>("estado_periodo");
        dataSourceBuilder.MapEnum<JournalEntryStatus>("estado_asiento");
        await using var dataSource = dataSourceBuilder.Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(dataSource, npgsqlOptions =>
            {
                npgsqlOptions.MapEnum<AccountNature>("naturaleza_cuenta");
                npgsqlOptions.MapEnum<AccountType>("tipo_cuenta");
                npgsqlOptions.MapEnum<PeriodStatus>("estado_periodo");
                npgsqlOptions.MapEnum<JournalEntryStatus>("estado_asiento");
            })
            .Options;

        await using var context = new AppDbContext(options, new FixedCurrentCompanyContext(1));
        await context.Database.MigrateAsync();

        const long companyId = 1;

        var currency = Currency.Create("USD", "US Dollar", 2);
        context.Currencies.Add(currency);

        var debitAccount = Account.Create(companyId, "1000", "Cash", AccountNature.Debit, AccountType.Asset, null, true);
        var creditAccount = Account.Create(companyId, "4000", "Revenue", AccountNature.Credit, AccountType.Revenue, null, true);
        context.Accounts.Add(debitAccount);
        context.Accounts.Add(creditAccount);

        var voucherType = VoucherType.Create(companyId, "GEN", "General", VoucherNumberingMode.Manual);
        context.VoucherTypes.Add(voucherType);

        var period = AccountingPeriod.Create(companyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        context.AccountingPeriods.Add(period);

        await context.SaveChangesAsync();

        var journalEntries = new JournalEntryRepository(context);
        var periods = new AccountingPeriodRepository(context);
        var voucherTypes = new VoucherTypeRepository(context);
        var service = new PostJournalEntryService(
            journalEntries, periods, voucherTypes,
            new AccountRepository(context), new CostCenterRepository(context), new PartyRepository(context), new AuditTrailWriter(context),
            new FixedCurrentCompanyContext(companyId));

        JournalEntryLineRequest[] Lines() => new[]
        {
            new JournalEntryLineRequest(debitAccount.Id, null, null, currency.Id, 100m, 0m),
            new JournalEntryLineRequest(creditAccount.Id, null, null, currency.Id, 0m, 100m)
        };

        var firstRequest = new PostJournalEntryRequest(
            companyId, voucherType.Id, period.Id, new DateOnly(2026, 1, 15), "First", "tester", Lines(),
            SourceModule: "test", SourceReference: "MANUAL-1", SequenceNumber: 1);
        var secondRequest = new PostJournalEntryRequest(
            companyId, voucherType.Id, period.Id, new DateOnly(2026, 1, 16), "Second", "tester", Lines(),
            SourceModule: "test", SourceReference: "MANUAL-2", SequenceNumber: 1);

        await service.PostAsync(firstRequest, CancellationToken.None);

        await Assert.ThrowsAsync<JournalEntrySequenceConflictException>(
            () => service.PostAsync(secondRequest, CancellationToken.None));

        var auditCount = await context.AuditTrailEntries.CountAsync(a => a.CompanyId == companyId);
        Assert.Equal(1, auditCount);
    }
}
