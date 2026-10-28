using System.Net;
using System.Net.Http.Json;
using Api.Controllers;

namespace Api.IntegrationTests;

[Collection(ApiTestCollection.Name)]
public sealed class UsersRegisterErrorHandlingTests
{
    private readonly ApiTestEnvironment _environment;

    public UsersRegisterErrorHandlingTests(ApiTestEnvironment environment) => _environment = environment;

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        using var factory = new AuthApiFactory(_environment);
        using var client = factory.CreateClient();
        var body = new UsersController.RegisterBody("dup-endpoint@acme.test", "s3cret-password", "Acme", "Acme MX", "MX");
        await client.PostAsJsonAsync("/api/users/register", body);

        var response = await client.PostAsJsonAsync("/api/users/register", body);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_WhitespaceOnlyTenantName_PassesRequiredButFailsDomainGuard_Returns400()
    {
        using var factory = new AuthApiFactory(_environment);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/users/register", new UsersController.RegisterBody(
            "whitespace-tenant@acme.test", "s3cret-password", "   ", "Acme MX", "MX"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_TooManyRequestsFromSameIp_Returns429()
    {
        using var factory = new AuthApiFactory(_environment);
        using var client = factory.CreateClient();

        HttpResponseMessage? last = null;
        for (var i = 0; i < 6; i++)
            last = await client.PostAsJsonAsync("/api/users/register", new UsersController.RegisterBody(
                $"rate-{i}@acme.test", "s3cret-password", "Acme", "Acme MX", "MX"));

        Assert.Equal(HttpStatusCode.TooManyRequests, last!.StatusCode);
    }
}
