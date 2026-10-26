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
public sealed class JournalEntriesEndpointTests : IClassFixture<AccountingApiFactory>
{
    private readonly AccountingApiFactory _factory;
    private readonly AccountingApiTestEnvironment _environment;

    public JournalEntriesEndpointTests(AccountingApiTestEnvironment environment)
    {
        _environment = environment;
        _factory = new AccountingApiFactory(environment);
    }

    private sealed record Fixture(string CompanyPublicId, string Jwt, long CompanyId, long VoucherTypeId, long PeriodId,
        Guid VoucherTypePublicId, Guid PeriodPublicId, Guid DebitAccountPublicId, Guid CreditAccountPublicId,
        Guid ThirdAccountPublicId, long ThirdAccountId, Guid CostCenterPublicId, long CostCenterId, Guid PartyPublicId, long PartyId);

    private async Task<Fixture> SeedAsync(HttpClient client, string emailSuffix)
    {
        using var scope = _factory.Services.CreateScope();
        var email = $"journal-{emailSuffix}@acme.test";
        var user = await scope.ServiceProvider.GetRequiredService<RegisterUserService>()
            .RegisterAsync(new RegisterUserRequest(email, "s3cret-password"), CancellationToken.None);

        var company = await scope.ServiceProvider.GetRequiredService<CreateTenantWithFirstCompanyService>()
            .CreateAsync(new CreateTenantWithFirstCompanyRequest($"Acme {emailSuffix}", $"Acme Co {emailSuffix}", "CO", user.Id), CancellationToken.None);

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

        var currency = await context.Currencies.FirstOrDefaultAsync(c => c.Code == "USD")
            ?? Currency.Create("USD", "US Dollar", 2);
        if (context.Entry(currency).State == EntityState.Detached)
            context.Currencies.Add(currency);

        var debitAccount = Account.Create(company.CompanyId, $"1000-{emailSuffix}", "Cash", AccountNature.Debit, AccountType.Asset, null, true);
        var creditAccount = Account.Create(company.CompanyId, $"4000-{emailSuffix}", "Revenue", AccountNature.Credit, AccountType.Revenue, null, true);
        var thirdAccount = Account.Create(company.CompanyId, $"4100-{emailSuffix}", "Other revenue", AccountNature.Credit, AccountType.Revenue, null, true);
        var costCenter = CostCenter.Create(company.CompanyId, $"CC-{emailSuffix}", "Sales");
        var party = Party.Create(company.CompanyId, $"P-{emailSuffix}", "Acme Client");
        var voucherType = VoucherType.Create(company.CompanyId, $"GEN-{emailSuffix}", "General", VoucherNumberingMode.Automatic);
        var period = AccountingPeriod.Create(company.CompanyId, $"2026-01-{emailSuffix}", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        context.Accounts.AddRange(debitAccount, creditAccount, thirdAccount);
        context.CostCenters.Add(costCenter);
        context.Parties.Add(party);
        context.VoucherTypes.Add(voucherType);
        context.AccountingPeriods.Add(period);
        await context.SaveChangesAsync();

        var login = await client.PostAsJsonAsync("/api/auth/login", new AuthController.LoginBody(email, "s3cret-password"));
        login.EnsureSuccessStatusCode();
        var loginBody = await login.Content.ReadFromJsonAsync<AuthController.LoginResponse>();

        return new Fixture(
            company.CompanyPublicId.ToString(), loginBody!.AccessToken, company.CompanyId, voucherType.Id, period.Id,
            voucherType.PublicId, period.PublicId, debitAccount.PublicId, creditAccount.PublicId,
            thirdAccount.PublicId, thirdAccount.Id, costCenter.PublicId, costCenter.Id, party.PublicId, party.Id);
    }

    private sealed class FixedCompanyContext(long companyId) : Shared.ICurrentCompanyContext
    {
        public long CompanyId => companyId;
    }

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

    private static HttpRequestMessage PostEntryRequest(Fixture fixture, object body) =>
        Authorized(new HttpRequestMessage(HttpMethod.Post, $"/api/companies/{fixture.CompanyPublicId}/journal-entries")
        {
            Content = JsonContent.Create(body)
        }, fixture.Jwt);

    private static HttpRequestMessage Authorized(HttpRequestMessage request, string jwt)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        return request;
    }

    private static object ValidBody(Fixture f, string? sourceModule = null, string? sourceReference = null) => new
    {
        VoucherTypePublicId = f.VoucherTypePublicId,
        AccountingPeriodPublicId = f.PeriodPublicId,
        EntryDate = new DateOnly(2026, 1, 15),
        Description = "Test entry",
        Lines = new object[]
        {
            new { AccountPublicId = f.DebitAccountPublicId, CurrencyCode = "USD", DebitAmount = 100m, CreditAmount = 0m },
            new { AccountPublicId = f.CreditAccountPublicId, CurrencyCode = "USD", DebitAmount = 0m, CreditAmount = 100m }
        },
        SourceModule = sourceModule,
        SourceReference = sourceReference
    };

    [Fact]
    public async Task Post_ValidEntry_Returns201WithPublicId()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "post-valid");

        var response = await client.SendAsync(PostEntryRequest(fixture, ValidBody(fixture)));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JournalEntryResult>();
        Assert.NotEqual(Guid.Empty, result!.PublicId);
        Assert.Equal("Posted", result.Status);
    }

    // El dominio lanza InvalidOperationException genérica (JournalEntry.Post), no un tipo
    // dedicado — AccountingExceptionHandler (Task 15) no la mapea a propósito porque el mismo
    // tipo también significa "período no encontrado" en otros Services; conflar ambos casos en
    // un único código HTTP sería peor que el 500 genérico actual. Pendiente como limitación
    // conocida en tasks/todo-accounting-api.md hasta que el dominio distinga ambos casos.
    [Fact]
    public async Task Post_UnbalancedEntry_Returns500()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "post-unbalanced");

        var body = new
        {
            VoucherTypePublicId = fixture.VoucherTypePublicId,
            AccountingPeriodPublicId = fixture.PeriodPublicId,
            EntryDate = new DateOnly(2026, 1, 15),
            Description = "Unbalanced",
            Lines = new object[]
            {
                new { AccountPublicId = fixture.DebitAccountPublicId, CurrencyCode = "USD", DebitAmount = 100m, CreditAmount = 0m },
                new { AccountPublicId = fixture.CreditAccountPublicId, CurrencyCode = "USD", DebitAmount = 0m, CreditAmount = 50m }
            }
        };

        var response = await client.SendAsync(PostEntryRequest(fixture, body));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task Post_RetriedWithSameSourceReference_ReturnsSameEntry_DoesNotDuplicate()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "post-idempotent");
        var body = ValidBody(fixture, sourceModule: "facturacion", sourceReference: "FAC-1");

        var first = await client.SendAsync(PostEntryRequest(fixture, body));
        var second = await client.SendAsync(PostEntryRequest(fixture, body));

        first.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        var firstResult = await first.Content.ReadFromJsonAsync<JournalEntryResult>();
        var secondResult = await second.Content.ReadFromJsonAsync<JournalEntryResult>();
        Assert.Equal(firstResult!.PublicId, secondResult!.PublicId);
    }

    [Fact]
    public async Task Post_LineAccountFromAnotherCompany_Returns404()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "post-cross-company");
        var otherCompany = await SeedAsync(client, "post-cross-company-other");

        var body = new
        {
            VoucherTypePublicId = fixture.VoucherTypePublicId,
            AccountingPeriodPublicId = fixture.PeriodPublicId,
            EntryDate = new DateOnly(2026, 1, 15),
            Description = "Cross company",
            Lines = new object[]
            {
                new { AccountPublicId = otherCompany.DebitAccountPublicId, CurrencyCode = "USD", DebitAmount = 100m, CreditAmount = 0m },
                new { AccountPublicId = fixture.CreditAccountPublicId, CurrencyCode = "USD", DebitAmount = 0m, CreditAmount = 100m }
            }
        };

        var response = await client.SendAsync(PostEntryRequest(fixture, body));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithUnknownCompanyPublicId_Returns404()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "post-unknown-company");

        var request = Authorized(new HttpRequestMessage(HttpMethod.Post, $"/api/companies/{Guid.NewGuid()}/journal-entries")
        {
            Content = JsonContent.Create(ValidBody(fixture))
        }, fixture.Jwt);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithoutMembershipInCompany_Returns404()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "post-no-membership-a");
        var otherCompany = await SeedAsync(client, "post-no-membership-b");

        var request = Authorized(new HttpRequestMessage(HttpMethod.Post, $"/api/companies/{otherCompany.CompanyPublicId}/journal-entries")
        {
            Content = JsonContent.Create(ValidBody(fixture))
        }, fixture.Jwt);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Regresión del fix de batch-resolution (ResolveLinesAsync): con 3 líneas y referencias
    // distintas por línea, confirma que cada línea persiste con su propia cuenta/centro de
    // costo/tercero — no que el batch por diccionario haya mezclado la línea 1 con la 3.
    [Fact]
    public async Task Post_WithMultipleLinesAndMixedReferences_ResolvesEachLineToItsOwnAccount()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "post-multiline");

        var body = new
        {
            VoucherTypePublicId = fixture.VoucherTypePublicId,
            AccountingPeriodPublicId = fixture.PeriodPublicId,
            EntryDate = new DateOnly(2026, 1, 15),
            Description = "Multi-line with mixed references",
            Lines = new object[]
            {
                new { AccountPublicId = fixture.DebitAccountPublicId, CurrencyCode = "USD", DebitAmount = 100m, CreditAmount = 0m },
                new { AccountPublicId = fixture.CreditAccountPublicId, CostCenterPublicId = fixture.CostCenterPublicId, CurrencyCode = "USD", DebitAmount = 0m, CreditAmount = 60m },
                new { AccountPublicId = fixture.ThirdAccountPublicId, PartyPublicId = fixture.PartyPublicId, CurrencyCode = "USD", DebitAmount = 0m, CreditAmount = 40m }
            }
        };

        var response = await client.SendAsync(PostEntryRequest(fixture, body));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JournalEntryResult>();

        await using var context = NewContext(fixture.CompanyId);
        var entry = await context.JournalEntries.Include(e => e.Lines).FirstAsync(e => e.PublicId == result!.PublicId);
        var lines = entry.Lines.OrderBy(l => l.LineNumber).ToList();

        Assert.Equal(3, lines.Count);
        Assert.Null(lines[0].CostCenterId);
        Assert.Null(lines[0].PartyId);
        Assert.Equal(fixture.CostCenterId, lines[1].CostCenterId);
        Assert.Null(lines[1].PartyId);
        Assert.Equal(fixture.PartyId, lines[2].PartyId);
        Assert.Equal(fixture.ThirdAccountId, lines[2].AccountId);
        Assert.Null(lines[2].CostCenterId);
    }

    [Fact]
    public async Task Reverse_PostedEntry_Returns201WithNewEntry()
    {
        using var client = _factory.CreateClient();
        var fixture = await SeedAsync(client, "reverse");

        var posted = await client.SendAsync(PostEntryRequest(fixture, ValidBody(fixture)));
        var postedResult = await posted.Content.ReadFromJsonAsync<JournalEntryResult>();

        var reverseBody = new { EntryDate = new DateOnly(2026, 1, 16), Description = "Reversal" };
        var reverseRequest = Authorized(new HttpRequestMessage(
            HttpMethod.Post, $"/api/companies/{fixture.CompanyPublicId}/journal-entries/{postedResult!.PublicId}/reversal")
        {
            Content = JsonContent.Create(reverseBody)
        }, fixture.Jwt);

        var response = await client.SendAsync(reverseRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var reversed = await response.Content.ReadFromJsonAsync<JournalEntryResult>();
        Assert.NotEqual(postedResult.PublicId, reversed!.PublicId);
    }
}
