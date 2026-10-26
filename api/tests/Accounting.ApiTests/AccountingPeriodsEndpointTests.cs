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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Users.DTOs;
using Users.Services;

namespace Accounting.ApiTests;

[Collection(AccountingApiTestCollection.Name)]
public sealed class AccountingPeriodsEndpointTests : IClassFixture<AccountingApiFactory>
{
    private readonly AccountingApiFactory _factory;
    private readonly AccountingApiTestEnvironment _environment;

    public AccountingPeriodsEndpointTests(AccountingApiTestEnvironment environment)
    {
        _environment = environment;
        _factory = new AccountingApiFactory(environment);
    }

    private sealed record Fixture(
        string CompanyPublicId, string Jwt, long CompanyId, long CreatorUserId,
        Guid AutomaticVoucherTypePublicId, long ManualVoucherTypeId, Guid ManualVoucherTypePublicId,
        long PeriodId, Guid PeriodPublicId, Guid DebitAccountPublicId, Guid CreditAccountPublicId, Guid NonPostingAccountPublicId);

    private AppDbContext NewContext(long companyId)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_environment.PostgresConnectionString);
        dataSourceBuilder.MapEnum<AccountNature>("naturaleza_cuenta");
        dataSourceBuilder.MapEnum<AccountType>("tipo_cuenta");
        dataSourceBuilder.MapEnum<PeriodStatus>("estado_periodo");
        dataSourceBuilder.MapEnum<JournalEntryStatus>("estado_asiento");
        var dataSource = dataSourceBuilder.Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(dataSource, o =>
            {
                o.MapEnum<AccountNature>("naturaleza_cuenta");
                o.MapEnum<AccountType>("tipo_cuenta");
                o.MapEnum<PeriodStatus>("estado_periodo");
                o.MapEnum<JournalEntryStatus>("estado_asiento");
            })
            .Options;

        return new AppDbContext(options, new FixedCompanyContext(companyId));
    }

    private async Task<string> RegisterSecondAdminAndLoginAsync(long companyId, long creatorUserId, string suffix)
    {
        using var scope = _factory.Services.CreateScope();
        var email = $"period-{suffix}-resolver@acme.test";
        var user = await scope.ServiceProvider.GetRequiredService<RegisterUserService>()
            .RegisterAsync(new RegisterUserRequest(email, "s3cret-password"), CancellationToken.None);

        await scope.ServiceProvider.GetRequiredService<AddMembershipService>().AddAsync(
            new AddMembershipRequest(creatorUserId, user.Id, companyId, Companies.Domain.Enums.CompanyMembershipRole.Admin),
            CancellationToken.None);

        using var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new AuthController.LoginBody(email, "s3cret-password"));
        login.EnsureSuccessStatusCode();
        var loginBody = await login.Content.ReadFromJsonAsync<AuthController.LoginResponse>();
        return loginBody!.AccessToken;
    }

    private sealed class FixedCompanyContext(long companyId) : Shared.ICurrentCompanyContext
    {
        public long CompanyId => companyId;
        public string Role => "Admin";
    }

    private async Task<Fixture> SeedAsync(HttpClient client, string suffix)
    {
        using var scope = _factory.Services.CreateScope();
        var email = $"period-{suffix}@acme.test";
        var user = await scope.ServiceProvider.GetRequiredService<RegisterUserService>()
            .RegisterAsync(new RegisterUserRequest(email, "s3cret-password"), CancellationToken.None);

        var company = await scope.ServiceProvider.GetRequiredService<CreateTenantWithFirstCompanyService>()
            .CreateAsync(new CreateTenantWithFirstCompanyRequest($"Acme {suffix}", $"Acme Co {suffix}", "CO", user.Id), CancellationToken.None);

        await using var context = NewContext(company.CompanyId);

        var currency = await context.Currencies.FirstOrDefaultAsync(c => c.Code == "USD") ?? Currency.Create("USD", "US Dollar", 2);
        if (context.Entry(currency).State == EntityState.Detached)
            context.Currencies.Add(currency);

        var debitAccount = Account.Create(company.CompanyId, $"1000-{suffix}", "Cash", AccountNature.Debit, AccountType.Asset, null, true);
        var creditAccount = Account.Create(company.CompanyId, $"4000-{suffix}", "Revenue", AccountNature.Credit, AccountType.Revenue, null, true);
        var nonPostingAccount = Account.Create(company.CompanyId, $"1-{suffix}", "Assets (group)", AccountNature.Debit, AccountType.Asset, null, false);
        var automaticVoucherType = VoucherType.Create(company.CompanyId, $"GEN-{suffix}", "General", VoucherNumberingMode.Automatic);
        var manualVoucherType = VoucherType.Create(company.CompanyId, $"MAN-{suffix}", "Manual", VoucherNumberingMode.Manual);
        var period = AccountingPeriod.Create(company.CompanyId, $"2026-01-{suffix}", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        context.Accounts.AddRange(debitAccount, creditAccount, nonPostingAccount);
        context.VoucherTypes.AddRange(automaticVoucherType, manualVoucherType);
        context.AccountingPeriods.Add(period);
        await context.SaveChangesAsync();

        var login = await client.PostAsJsonAsync("/api/auth/login", new AuthController.LoginBody(email, "s3cret-password"));
        login.EnsureSuccessStatusCode();
        var loginBody = await login.Content.ReadFromJsonAsync<AuthController.LoginResponse>();

        return new Fixture(
            company.CompanyPublicId.ToString(), loginBody!.AccessToken, company.CompanyId, user.Id,
            automaticVoucherType.PublicId, manualVoucherType.Id, manualVoucherType.PublicId,
            period.Id, period.PublicId, debitAccount.PublicId, creditAccount.PublicId, nonPostingAccount.PublicId);
    }

    private static HttpRequestMessage Authorized(HttpRequestMessage request, string jwt)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        return request;
    }

    private static HttpRequestMessage PostJsonRequest(string path, string jwt, object body) =>
        Authorized(new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(body) }, jwt);

    private static object EntryBody(Fixture f, Guid voucherTypePublicId, string description, string? sourceModule = null, string? sourceReference = null, long? sequenceNumber = null) => new
    {
        VoucherTypePublicId = voucherTypePublicId,
        AccountingPeriodPublicId = f.PeriodPublicId,
        EntryDate = new DateOnly(2026, 1, 15),
        Description = description,
        Lines = new object[]
        {
            new { AccountPublicId = f.DebitAccountPublicId, CurrencyCode = "USD", DebitAmount = 100m, CreditAmount = 0m },
            new { AccountPublicId = f.CreditAccountPublicId, CurrencyCode = "USD", DebitAmount = 0m, CreditAmount = 100m }
        },
        SourceModule = sourceModule,
        SourceReference = sourceReference,
        SequenceNumber = sequenceNumber
    };

    [Fact]
    public async Task Close_PeriodWithoutDrafts_Returns200()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "close-ok");

        var response = await client.SendAsync(Authorized(
            new HttpRequestMessage(HttpMethod.Post, $"/api/companies/{fixture.CompanyPublicId}/accounting-periods/{fixture.PeriodPublicId}/closure"),
            fixture.Jwt));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AccountingPeriodResult>();
        Assert.Equal("Closed", result!.Status);
    }

    [Fact]
    public async Task Close_PeriodWithDraftEntries_Returns409()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "close-drafts");

        await using (var context = NewContext(fixture.CompanyId))
        {
            var draft = JournalEntry.Create(fixture.CompanyId, fixture.ManualVoucherTypeId, fixture.PeriodId, new DateOnly(2026, 1, 15), "Draft entry");
            draft.AddLine((await context.Accounts.FirstAsync()).Id, null, null, (await context.Currencies.FirstAsync()).Id, 10m, 0m);
            context.JournalEntries.Add(draft);
            await context.SaveChangesAsync();
        }

        var response = await client.SendAsync(Authorized(
            new HttpRequestMessage(HttpMethod.Post, $"/api/companies/{fixture.CompanyPublicId}/accounting-periods/{fixture.PeriodPublicId}/closure"),
            fixture.Jwt));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("El período tiene asientos en borrador", problem!.Title);
    }

    [Fact]
    public async Task Post_IntoClosedPeriod_Returns409()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "post-closed");

        var closeResponse = await client.SendAsync(Authorized(
            new HttpRequestMessage(HttpMethod.Post, $"/api/companies/{fixture.CompanyPublicId}/accounting-periods/{fixture.PeriodPublicId}/closure"),
            fixture.Jwt));
        closeResponse.EnsureSuccessStatusCode();

        var response = await client.SendAsync(PostJsonRequest(
            $"/api/companies/{fixture.CompanyPublicId}/journal-entries", fixture.Jwt,
            EntryBody(fixture, fixture.AutomaticVoucherTypePublicId, "Into closed period")));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Período contable cerrado", problem!.Title);
    }

    [Fact]
    public async Task RequestReopen_ThenResolveApprove_ReopensPeriod()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "reopen-approve");

        var closeResponse = await client.SendAsync(Authorized(
            new HttpRequestMessage(HttpMethod.Post, $"/api/companies/{fixture.CompanyPublicId}/accounting-periods/{fixture.PeriodPublicId}/closure"),
            fixture.Jwt));
        closeResponse.EnsureSuccessStatusCode();

        var requestResponse = await client.SendAsync(PostJsonRequest(
            $"/api/companies/{fixture.CompanyPublicId}/accounting-periods/{fixture.PeriodPublicId}/reopen-requests",
            fixture.Jwt, new { Reason = "Correction needed" }));
        requestResponse.EnsureSuccessStatusCode();
        var reopenRequest = await requestResponse.Content.ReadFromJsonAsync<PeriodReopenRequestResult>();

        var resolverJwt = await RegisterSecondAdminAndLoginAsync(fixture.CompanyId, fixture.CreatorUserId, "reopen-approve");
        var resolveResponse = await client.SendAsync(PostJsonRequest(
            $"/api/companies/{fixture.CompanyPublicId}/period-reopen-requests/{reopenRequest!.PublicId}/resolution",
            resolverJwt, new { Approve = true }));

        Assert.Equal(HttpStatusCode.OK, resolveResponse.StatusCode);
        var resolved = await resolveResponse.Content.ReadFromJsonAsync<PeriodReopenRequestResult>();
        Assert.Equal("Approved", resolved!.Status);

        var secondClose = await client.SendAsync(Authorized(
            new HttpRequestMessage(HttpMethod.Post, $"/api/companies/{fixture.CompanyPublicId}/accounting-periods/{fixture.PeriodPublicId}/closure"),
            fixture.Jwt));
        Assert.Equal(HttpStatusCode.OK, secondClose.StatusCode);
    }

    [Fact]
    public async Task RequestReopen_ThenResolveReject_KeepsPeriodClosed()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "reopen-reject");

        await client.SendAsync(Authorized(
            new HttpRequestMessage(HttpMethod.Post, $"/api/companies/{fixture.CompanyPublicId}/accounting-periods/{fixture.PeriodPublicId}/closure"),
            fixture.Jwt));

        var requestResponse = await client.SendAsync(PostJsonRequest(
            $"/api/companies/{fixture.CompanyPublicId}/accounting-periods/{fixture.PeriodPublicId}/reopen-requests",
            fixture.Jwt, new { Reason = "Just checking" }));
        var reopenRequest = await requestResponse.Content.ReadFromJsonAsync<PeriodReopenRequestResult>();

        var resolverJwt = await RegisterSecondAdminAndLoginAsync(fixture.CompanyId, fixture.CreatorUserId, "reopen-reject");
        var resolveResponse = await client.SendAsync(PostJsonRequest(
            $"/api/companies/{fixture.CompanyPublicId}/period-reopen-requests/{reopenRequest!.PublicId}/resolution",
            resolverJwt, new { Approve = false, Notes = "Not justified" }));

        Assert.Equal(HttpStatusCode.OK, resolveResponse.StatusCode);
        var resolved = await resolveResponse.Content.ReadFromJsonAsync<PeriodReopenRequestResult>();
        Assert.Equal("Rejected", resolved!.Status);

        var secondClose = await client.SendAsync(Authorized(
            new HttpRequestMessage(HttpMethod.Post, $"/api/companies/{fixture.CompanyPublicId}/accounting-periods/{fixture.PeriodPublicId}/closure"),
            fixture.Jwt));
        Assert.Equal(HttpStatusCode.InternalServerError, secondClose.StatusCode);
    }

    [Fact]
    public async Task ResolveReopen_BySameUserWhoRequestedIt_Returns403()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "reopen-self");

        await client.SendAsync(Authorized(
            new HttpRequestMessage(HttpMethod.Post, $"/api/companies/{fixture.CompanyPublicId}/accounting-periods/{fixture.PeriodPublicId}/closure"),
            fixture.Jwt));

        var requestResponse = await client.SendAsync(PostJsonRequest(
            $"/api/companies/{fixture.CompanyPublicId}/accounting-periods/{fixture.PeriodPublicId}/reopen-requests",
            fixture.Jwt, new { Reason = "Self approval attempt" }));
        var reopenRequest = await requestResponse.Content.ReadFromJsonAsync<PeriodReopenRequestResult>();

        var resolveResponse = await client.SendAsync(PostJsonRequest(
            $"/api/companies/{fixture.CompanyPublicId}/period-reopen-requests/{reopenRequest!.PublicId}/resolution",
            fixture.Jwt, new { Approve = true }));

        Assert.Equal(HttpStatusCode.Forbidden, resolveResponse.StatusCode);
    }

    [Fact]
    public async Task Close_AsAccountant_Returns403()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "close-accountant");

        var accountantEmail = "period-close-accountant-member@acme.test";
        using (var scope = _factory.Services.CreateScope())
        {
            var member = await scope.ServiceProvider.GetRequiredService<RegisterUserService>()
                .RegisterAsync(new RegisterUserRequest(accountantEmail, "s3cret-password"), CancellationToken.None);
            await scope.ServiceProvider.GetRequiredService<AddMembershipService>().AddAsync(
                new AddMembershipRequest(fixture.CreatorUserId, member.Id, fixture.CompanyId, Companies.Domain.Enums.CompanyMembershipRole.Accountant),
                CancellationToken.None);
        }

        var login = await client.PostAsJsonAsync("/api/auth/login", new AuthController.LoginBody(accountantEmail, "s3cret-password"));
        login.EnsureSuccessStatusCode();
        var accountantJwt = (await login.Content.ReadFromJsonAsync<AuthController.LoginResponse>())!.AccessToken;

        var response = await client.SendAsync(Authorized(
            new HttpRequestMessage(HttpMethod.Post, $"/api/companies/{fixture.CompanyPublicId}/accounting-periods/{fixture.PeriodPublicId}/closure"),
            accountantJwt));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithNonPostingAccount_Returns422()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "non-posting");

        var body = new
        {
            VoucherTypePublicId = fixture.AutomaticVoucherTypePublicId,
            AccountingPeriodPublicId = fixture.PeriodPublicId,
            EntryDate = new DateOnly(2026, 1, 15),
            Description = "Group account",
            Lines = new object[]
            {
                new { AccountPublicId = fixture.NonPostingAccountPublicId, CurrencyCode = "USD", DebitAmount = 100m, CreditAmount = 0m },
                new { AccountPublicId = fixture.CreditAccountPublicId, CurrencyCode = "USD", DebitAmount = 0m, CreditAmount = 100m }
            }
        };

        var response = await client.SendAsync(PostJsonRequest($"/api/companies/{fixture.CompanyPublicId}/journal-entries", fixture.Jwt, body));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("La cuenta no admite contabilización", problem!.Title);
    }

    [Fact]
    public async Task Post_SameSourceReferenceDifferentContent_Returns409()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "idempotency-conflict");

        var first = await client.SendAsync(PostJsonRequest(
            $"/api/companies/{fixture.CompanyPublicId}/journal-entries", fixture.Jwt,
            EntryBody(fixture, fixture.AutomaticVoucherTypePublicId, "First", sourceModule: "facturacion", sourceReference: "FAC-9")));
        first.EnsureSuccessStatusCode();

        var secondBody = EntryBody(fixture, fixture.AutomaticVoucherTypePublicId, "Different description", sourceModule: "facturacion", sourceReference: "FAC-9");
        var second = await client.SendAsync(PostJsonRequest($"/api/companies/{fixture.CompanyPublicId}/journal-entries", fixture.Jwt, secondBody));

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        var problem = await second.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Conflicto de idempotencia", problem!.Title);
    }

    [Fact]
    public async Task Post_ManualVoucherTypeWithDuplicateSequence_Returns409()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "sequence-conflict");

        var first = await client.SendAsync(PostJsonRequest(
            $"/api/companies/{fixture.CompanyPublicId}/journal-entries", fixture.Jwt,
            EntryBody(fixture, fixture.ManualVoucherTypePublicId, "First manual", sequenceNumber: 1)));
        first.EnsureSuccessStatusCode();

        var second = await client.SendAsync(PostJsonRequest(
            $"/api/companies/{fixture.CompanyPublicId}/journal-entries", fixture.Jwt,
            EntryBody(fixture, fixture.ManualVoucherTypePublicId, "Second manual", sequenceNumber: 1)));

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        var problem = await second.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Conflicto de numeración", problem!.Title);
    }
}
