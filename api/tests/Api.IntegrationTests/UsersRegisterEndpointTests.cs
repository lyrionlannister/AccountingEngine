using System.Net;
using System.Net.Http.Json;
using Api.Controllers;

namespace Api.IntegrationTests;

[Collection(ApiTestCollection.Name)]
public sealed class UsersRegisterEndpointTests
{
    private readonly ApiTestEnvironment _environment;

    public UsersRegisterEndpointTests(ApiTestEnvironment environment) => _environment = environment;

    [Fact]
    public async Task Register_ValidBody_Returns201WithOnlyGuidIds()
    {
        using var factory = new AuthApiFactory(_environment);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/users/register", new UsersController.RegisterBody(
            "new-owner@acme.test", "s3cret-password", "Acme", "Acme MX", "MX"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UsersController.RegisterResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.OwnerPublicId);
        Assert.NotEqual(Guid.Empty, body.TenantPublicId);
        Assert.NotEqual(Guid.Empty, body.CompanyPublicId);
        Assert.NotEqual(Guid.Empty, body.MembershipPublicId);

        var raw = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("\"id\"", raw, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("", "s3cret-password", "Acme", "Acme MX", "MX")]
    [InlineData("not-an-email", "s3cret-password", "Acme", "Acme MX", "MX")]
    [InlineData("valid@acme.test", "s3cret-password", "Acme", "Acme MX", "mx")]
    [InlineData("valid@acme.test", "s3cret-password", "Acme", "Acme MX", "MEX")]
    public async Task Register_InvalidBody_Returns400(
        string email, string password, string tenantName, string companyName, string countryCode)
    {
        using var factory = new AuthApiFactory(_environment);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/users/register", new UsersController.RegisterBody(
            email, password, tenantName, companyName, countryCode));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
