using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Users.DTOs;
using Users.Services;

namespace Api.IntegrationTests;

[Collection(ApiTestCollection.Name)]
public sealed class AuthEndpointTests : IClassFixture<AuthApiFactory>
{
    private readonly AuthApiFactory _factory;

    public AuthEndpointTests(ApiTestEnvironment environment) => _factory = new AuthApiFactory(environment);

    private async Task RegisterUserAsync(string email, string password)
    {
        using var scope = _factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<RegisterUserService>()
            .RegisterAsync(new RegisterUserRequest(email, password), CancellationToken.None);
    }

    private static HttpRequestMessage LoginRequestFrom(string email, string password, string? ip = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new AuthController.LoginBody(email, password))
        };
        if (ip is not null)
            request.Headers.Add(AuthApiFactory.TestRemoteIpHeader, ip);
        return request;
    }

    [Fact]
    public async Task Login_FromDifferentIpThanAnActiveSession_Returns409WithSpanishMessage()
    {
        const string email = "ip-conflict@acme.test";
        await RegisterUserAsync(email, "s3cret-password");
        using var client = _factory.CreateClient();

        var first = await client.SendAsync(LoginRequestFrom(email, "s3cret-password", "203.0.113.10"));
        first.EnsureSuccessStatusCode();

        var conflict = await client.SendAsync(LoginRequestFrom(email, "s3cret-password", "198.51.100.20"));

        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        var problem = await conflict.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Ya existe una sesión activa", problem!.Title);
        Assert.Equal(
            "Ya existe una sesión activa. Cierra sesión en tu otro dispositivo para iniciar aquí.",
            problem.Detail);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAccessToken()
    {
        const string email = "valid-login@acme.test";
        await RegisterUserAsync(email, "s3cret-password");
        using var client = _factory.CreateClient();

        var response = await client.SendAsync(LoginRequestFrom(email, "s3cret-password"));

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthController.LoginResponse>();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(body!.AccessToken);
        Assert.NotNull(jwt.Claims.FirstOrDefault(c => c.Type == "sid"));
    }

    [Fact]
    public async Task Logout_RevokesSession_SubsequentAuthenticatedRequestReturns401()
    {
        const string email = "logout-revokes@acme.test";
        await RegisterUserAsync(email, "s3cret-password");
        using var client = _factory.CreateClient();

        var loginResponse = await client.SendAsync(LoginRequestFrom(email, "s3cret-password"));
        var login = await loginResponse.Content.ReadFromJsonAsync<AuthController.LoginResponse>();

        using var firstLogout = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        firstLogout.Headers.Add("Authorization", $"Bearer {login!.AccessToken}");
        var firstLogoutResponse = await client.SendAsync(firstLogout);
        Assert.Equal(HttpStatusCode.NoContent, firstLogoutResponse.StatusCode);

        using var secondLogout = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        secondLogout.Headers.Add("Authorization", $"Bearer {login.AccessToken}");
        var secondLogoutResponse = await client.SendAsync(secondLogout);
        Assert.Equal(HttpStatusCode.Unauthorized, secondLogoutResponse.StatusCode);
    }

    [Fact]
    public async Task Login_11thAttemptWithinWindow_Returns429()
    {
        const string email = "rate-limited@acme.test";
        await RegisterUserAsync(email, "s3cret-password");
        using var client = _factory.CreateClient();

        HttpResponseMessage? last = null;
        for (var attempt = 0; attempt < 11; attempt++)
            last = await client.SendAsync(LoginRequestFrom(email, "s3cret-password", "203.0.113.50"));

        Assert.Equal(HttpStatusCode.TooManyRequests, last!.StatusCode);
    }
}
