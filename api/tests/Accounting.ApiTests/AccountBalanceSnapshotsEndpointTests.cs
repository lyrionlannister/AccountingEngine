using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Accounting.Domain;
using Accounting.Domain.Enums;
using Accounting.DTOs;
using Accounting.Persistence;
using Api.Controllers;
using Companies.DTOs;
using Companies.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Users.DTOs;
using Users.Services;

namespace Accounting.ApiTests;

[Collection(AccountingApiTestCollection.Name)]
public sealed class AccountBalanceSnapshotsEndpointTests : IClassFixture<AccountingApiFactory>
{
    private readonly AccountingApiFactory _factory;
    private readonly AccountingApiTestEnvironment _environment;

    public AccountBalanceSnapshotsEndpointTests(AccountingApiTestEnvironment environment)
    {
        _environment = environment;
        _factory = new AccountingApiFactory(environment);
    }

    private sealed record Fixture(string CompanyPublicId, string Jwt, long CompanyId, Guid PeriodPublicId, Guid AccountPublicId);

    private sealed class FixedCompanyContext(long companyId) : Shared.ICurrentCompanyContext
    {
        public long CompanyId => companyId;
    }

    private async Task<Fixture> SeedAsync(HttpClient client, string suffix)
    {
        using var scope = _factory.Services.CreateScope();
        var email = $"snapshot-{suffix}@acme.test";
        var user = await scope.ServiceProvider.GetRequiredService<RegisterUserService>()
            .RegisterAsync(new RegisterUserRequest(email, "s3cret-password"), CancellationToken.None);

        var company = await scope.ServiceProvider.GetRequiredService<CreateTenantWithFirstCompanyService>()
            .CreateAsync(new CreateTenantWithFirstCompanyRequest($"Acme {suffix}", $"Acme Co {suffix}", "CO", user.Id), CancellationToken.None);

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_environment.PostgresConnectionString);
        dataSourceBuilder.MapEnum<AccountNature>("naturaleza_cuenta");
        dataSourceBuilder.MapEnum<AccountType>("tipo_cuenta");
        dataSourceBuilder.MapEnum<PeriodStatus>("estado_periodo");
        dataSourceBuilder.MapEnum<JournalEntryStatus>("estado_asiento");
        await using var dataSource = dataSourceBuilder.Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(dataSource, o =>
            {
                o.MapEnum<AccountNature>("naturaleza_cuenta");
                o.MapEnum<AccountType>("tipo_cuenta");
                o.MapEnum<PeriodStatus>("estado_periodo");
                o.MapEnum<JournalEntryStatus>("estado_asiento");
            })
            .Options;

        await using var context = new AppDbContext(options, new FixedCompanyContext(company.CompanyId));

        var account = Account.Create(company.CompanyId, $"1000-{suffix}", "Cash", AccountNature.Debit, AccountType.Asset, null, true);
        var period = AccountingPeriod.Create(company.CompanyId, $"2026-01-{suffix}", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        context.Accounts.Add(account);
        context.AccountingPeriods.Add(period);
        await context.SaveChangesAsync();

        var login = await client.PostAsJsonAsync("/api/auth/login", new AuthController.LoginBody(email, "s3cret-password"));
        login.EnsureSuccessStatusCode();
        var loginBody = await login.Content.ReadFromJsonAsync<AuthController.LoginResponse>();

        return new Fixture(company.CompanyPublicId.ToString(), loginBody!.AccessToken, company.CompanyId, period.PublicId, account.PublicId);
    }

    private static HttpRequestMessage RecalculateRequest(Fixture f, Guid accountPublicId) =>
        Authorize(new HttpRequestMessage(HttpMethod.Post,
            $"/api/companies/{f.CompanyPublicId}/accounting-periods/{f.PeriodPublicId}/accounts/{accountPublicId}/balance-snapshot/recalculation"), f.Jwt);

    private static HttpRequestMessage Authorize(HttpRequestMessage request, string jwt)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        return request;
    }

    [Fact]
    public async Task Recalculate_NewSnapshot_CreatesIt()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "new");

        var response = await client.SendAsync(RecalculateRequest(fixture, fixture.AccountPublicId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AccountBalanceSnapshotResult>();
        Assert.Equal(0m, result!.DebitTotal);
        Assert.Equal(0m, result.CreditTotal);
    }

    [Fact]
    public async Task Recalculate_ExistingSnapshot_UpdatesInsteadOfDuplicating()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "existing");

        var first = await client.SendAsync(RecalculateRequest(fixture, fixture.AccountPublicId));
        first.EnsureSuccessStatusCode();
        var second = await client.SendAsync(RecalculateRequest(fixture, fixture.AccountPublicId));

        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var firstResult = await first.Content.ReadFromJsonAsync<AccountBalanceSnapshotResult>();
        var secondResult = await second.Content.ReadFromJsonAsync<AccountBalanceSnapshotResult>();
        Assert.True(secondResult!.CalculatedAt >= firstResult!.CalculatedAt);
    }

    [Fact]
    public async Task Recalculate_AccountFromAnotherCompany_Returns404()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "cross-a");
        var other = await SeedAsync(client, "cross-b");

        var response = await client.SendAsync(RecalculateRequest(fixture, other.AccountPublicId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
