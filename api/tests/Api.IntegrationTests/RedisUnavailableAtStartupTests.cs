using System.Net;
using System.Net.Http.Json;
using Api.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Users.DTOs;
using Users.Services;

namespace Api.IntegrationTests;

[Collection(ApiTestCollection.Name)]
public sealed class RedisUnavailableAtStartupTests : IClassFixture<RedisUnavailableAtStartupTests.Factory>
{
    private readonly Factory _factory;

    public RedisUnavailableAtStartupTests(ApiTestEnvironment environment) => _factory = new Factory(environment);

    [Fact]
    public async Task Login_WithRedisUnreachableSinceStartup_StillSucceedsViaPostgresFallback()
    {
        using var scope = _factory.Services.CreateScope();
        const string email = "redis-down-at-boot@acme.test";
        await scope.ServiceProvider.GetRequiredService<RegisterUserService>()
            .RegisterAsync(new RegisterUserRequest(email, "s3cret-password"), CancellationToken.None);

        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new AuthController.LoginBody(email, "s3cret-password"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthController.LoginResponse>();
        Assert.NotNull(body?.AccessToken);
    }

    public sealed class Factory : WebApplicationFactory<Program>
    {
        public Factory(ApiTestEnvironment environment)
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__Default", environment.PostgresConnectionString);
            Environment.SetEnvironmentVariable("ConnectionStrings__Redis", "127.0.0.1:1");
            Environment.SetEnvironmentVariable("Jwt__SigningKey", Convert.ToBase64String(new byte[32]));
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder) =>
            builder.ConfigureServices(services =>
                services.AddTransient<IStartupFilter, FakeRemoteIpStartupFilter>());

        private sealed class FakeRemoteIpStartupFilter : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
            {
                app.Use((context, nextMiddleware) =>
                {
                    context.Connection.RemoteIpAddress = IPAddress.Loopback;
                    return nextMiddleware();
                });
                next(app);
            };
        }
    }
}
