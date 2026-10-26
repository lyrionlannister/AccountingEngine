using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Api.IntegrationTests;

public sealed class AuthApiFactory : WebApplicationFactory<Program>
{
    public AuthApiFactory(ApiTestEnvironment environment)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", environment.PostgresConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__Redis", environment.RedisConnectionString);
        Environment.SetEnvironmentVariable("Jwt__SigningKey", Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureServices(services =>
            services.AddTransient<IStartupFilter, FakeRemoteIpStartupFilter>());

    public const string TestRemoteIpHeader = "X-Test-RemoteIp";

    private sealed class FakeRemoteIpStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use((context, nextMiddleware) =>
            {
                var testIp = context.Request.Headers[TestRemoteIpHeader].FirstOrDefault();
                context.Connection.RemoteIpAddress = testIp is not null ? IPAddress.Parse(testIp) : IPAddress.Loopback;
                return nextMiddleware();
            });
            next(app);
        };
    }
}
