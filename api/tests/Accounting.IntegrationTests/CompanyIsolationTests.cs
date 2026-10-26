using Accounting.Domain;
using Accounting.Domain.Enums;
using Accounting.DTOs;
using Accounting.Repositories;
using Accounting.Services;
using Companies.Domain;
using Companies.Domain.Enums;
using Companies.Persistence;
using Companies.Repositories;
using Companies.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Users.DTOs;
using Users.Persistence;
using Users.Repositories;
using Users.Services;

namespace Accounting.IntegrationTests;

// Companies y Accounting comparten una sola base física (ver compose.yaml). Ambos DbContext
// usan la tabla de historial de migraciones por defecto (__EFMigrationsHistory); esto es seguro
// porque los nombres de migración de cada módulo son únicos (InitialCompaniesSchema vs.
// InitialAccountingSchema/InitialCreate) y cada DbContext solo compara su propio conjunto de
// nombres de migración contra esa tabla al decidir qué aplicar.
[Collection(PostgresCollection.Name)]
public sealed class CompanyIsolationTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public CompanyIsolationTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ResolveAsync_WithoutMembership_FailsBeforeAnyAccountingQuery()
    {
        await using var companiesContext = await MigratedCompaniesContextAsync();

        var sut = new CurrentCompanyContext(new MembershipRepository(companiesContext));

        await Assert.ThrowsAsync<CompanyMembershipNotFoundException>(
            () => sut.ResolveAsync(userId: 999, companyId: 999, CancellationToken.None));
    }

    [Fact]
    public async Task GlobalFilter_NeverLeaksJournalEntriesAcrossCompanies()
    {
        await using var companiesContext = await MigratedCompaniesContextAsync();
        await using var usersContext = await MigratedUsersContextAsync();

        var users = new UserRepository(usersContext);
        var userA = await new RegisterUserService(users).RegisterAsync(
            new RegisterUserRequest("admin-a@acme.test", "s3cret-password"), CancellationToken.None);
        var userB = await new RegisterUserService(users).RegisterAsync(
            new RegisterUserRequest("admin-b@acme.test", "s3cret-password"), CancellationToken.None);

        var tenants = new TenantRepository(companiesContext);
        var (_, companyA, _) = await tenants.CreateWithFirstCompanyAsync(
            () => Tenant.Create("Tenant A"),
            tenantId => Company.Create(tenantId, "Company A", "CO"),
            companyId => Membership.Create(userA.Id, companyId, CompanyMembershipRole.Admin, DateTimeOffset.UtcNow),
            CancellationToken.None);
        var (_, companyB, _) = await tenants.CreateWithFirstCompanyAsync(
            () => Tenant.Create("Tenant B"),
            tenantId => Company.Create(tenantId, "Company B", "MX"),
            companyId => Membership.Create(userB.Id, companyId, CompanyMembershipRole.Admin, DateTimeOffset.UtcNow),
            CancellationToken.None);

        await using var database = await TestDatabase.CreateAsync(_postgres);

        var entryIdA = await CreateJournalEntryAsync(database, companyA.Id);
        var entryIdB = await CreateJournalEntryAsync(database, companyB.Id);

        await using var contextAsA = database.NewContext(companyA.Id);
        Assert.NotNull(await contextAsA.JournalEntries.FirstOrDefaultAsync(e => e.Id == entryIdA));
        Assert.Null(await contextAsA.JournalEntries.FirstOrDefaultAsync(e => e.Id == entryIdB));

        await using var contextAsB = database.NewContext(companyB.Id);
        Assert.NotNull(await contextAsB.JournalEntries.FirstOrDefaultAsync(e => e.Id == entryIdB));
        Assert.Null(await contextAsB.JournalEntries.FirstOrDefaultAsync(e => e.Id == entryIdA));
    }

    [Fact]
    public async Task PostAsync_WithAccountFromAnotherCompany_ThrowsReferenceNotFound()
    {
        // La FK de "cuentas" por sí sola solo prueba que la fila existe, no que es de la misma
        // empresa que el asiento — hallazgo de tenant-isolation sobre PostJournalEntryService.
        await using var database = await TestDatabase.CreateAsync(_postgres);
        const long companyA = 1;
        const long companyB = 2;

        await using var contextA = database.NewContext(companyA);
        var accountOfCompanyA = Account.Create(companyA, "1000", "Cash", AccountNature.Debit, AccountType.Asset, null, true);
        contextA.Accounts.Add(accountOfCompanyA);
        await contextA.SaveChangesAsync();

        await using var contextB = database.NewContext(companyB);
        var currency = Currency.Create("USD", "US Dollar", 2);
        var ownAccountOfCompanyB = Account.Create(companyB, "4000", "Revenue", AccountNature.Credit, AccountType.Revenue, null, true);
        contextB.Currencies.Add(currency);
        contextB.Accounts.Add(ownAccountOfCompanyB);
        var voucherType = VoucherType.Create(companyB, "GEN", "General", VoucherNumberingMode.Automatic);
        contextB.VoucherTypes.Add(voucherType);
        var period = AccountingPeriod.Create(companyB, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        contextB.AccountingPeriods.Add(period);
        await contextB.SaveChangesAsync();

        var service = new PostJournalEntryService(
            new JournalEntryRepository(contextB), new AccountingPeriodRepository(contextB), new VoucherTypeRepository(contextB),
            new AccountRepository(contextB), new CostCenterRepository(contextB), new PartyRepository(contextB), new AuditTrailWriter(contextB),
            new FixedCurrentCompanyContext(companyB));

        // companyB intenta contabilizar contra una cuenta que en realidad es de companyA.
        var request = new PostJournalEntryRequest(
            companyB, voucherType.Id, period.Id, new DateOnly(2026, 1, 15), "Cross-company attempt", "tester",
            new List<JournalEntryLineRequest>
            {
                new(accountOfCompanyA.Id, null, null, currency.Id, 100m, 0m),
                new(ownAccountOfCompanyB.Id, null, null, currency.Id, 0m, 100m)
            });

        await Assert.ThrowsAsync<JournalEntryLineReferenceNotFoundException>(
            () => service.PostAsync(request, CancellationToken.None));
    }

    private async Task<CompaniesDbContext> MigratedCompaniesContextAsync()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_postgres);
        dataSourceBuilder.MapEnum<CompanyMembershipRole>("rol_membresia");
        var dataSource = dataSourceBuilder.Build();

        var options = new DbContextOptionsBuilder<CompaniesDbContext>()
            .UseNpgsql(dataSource, b => b.MapEnum<CompanyMembershipRole>("rol_membresia"))
            .Options;

        var context = new CompaniesDbContext(options);
        await context.Database.MigrateAsync();
        return context;
    }

    private async Task<UsersDbContext> MigratedUsersContextAsync()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>().UseNpgsql(_postgres).Options;

        var context = new UsersDbContext(options);
        await context.Database.MigrateAsync();
        return context;
    }

    private static async Task<long> CreateJournalEntryAsync(TestDatabase database, long companyId)
    {
        await using var context = database.NewContext(companyId);

        var currency = await context.Currencies.FirstOrDefaultAsync(c => c.Code == "USD")
            ?? Currency.Create("USD", "US Dollar", 2);
        if (currency.Id == 0)
            context.Currencies.Add(currency);

        var debitAccount = Account.Create(companyId, $"1000-{companyId}", "Cash", AccountNature.Debit, AccountType.Asset, null, true);
        var creditAccount = Account.Create(companyId, $"4000-{companyId}", "Revenue", AccountNature.Credit, AccountType.Revenue, null, true);
        context.Accounts.Add(debitAccount);
        context.Accounts.Add(creditAccount);

        var voucherType = VoucherType.Create(companyId, "GEN", "General", VoucherNumberingMode.Automatic);
        context.VoucherTypes.Add(voucherType);

        var period = AccountingPeriod.Create(companyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        context.AccountingPeriods.Add(period);

        await context.SaveChangesAsync();

        var service = new PostJournalEntryService(
            new JournalEntryRepository(context), new AccountingPeriodRepository(context), new VoucherTypeRepository(context),
            new AccountRepository(context), new CostCenterRepository(context), new PartyRepository(context), new AuditTrailWriter(context),
            new FixedCurrentCompanyContext(companyId));

        var request = new PostJournalEntryRequest(
            companyId, voucherType.Id, period.Id, new DateOnly(2026, 1, 15), $"Entry for company {companyId}", "tester",
            new List<JournalEntryLineRequest>
            {
                new(debitAccount.Id, null, null, currency.Id, 100m, 0m),
                new(creditAccount.Id, null, null, currency.Id, 0m, 100m)
            });

        var result = await service.PostAsync(request, CancellationToken.None);
        return result.Id;
    }
}
