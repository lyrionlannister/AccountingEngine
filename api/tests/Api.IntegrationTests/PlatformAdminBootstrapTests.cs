using Companies.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Users.Services;

namespace Api.IntegrationTests;

[Collection(ApiTestCollection.Name)]
public sealed class PlatformAdminBootstrapTests
{
    private readonly ApiTestEnvironment _environment;

    public PlatformAdminBootstrapTests(ApiTestEnvironment environment) => _environment = environment;

    [Fact]
    public async Task Boot_WithPlatformAdminEnvVarsSet_CreatesAndPromotesTheUser()
    {
        const string email = "platform-admin-boot@acme.test";
        Environment.SetEnvironmentVariable("PlatformAdmin__Email", email);
        Environment.SetEnvironmentVariable("PlatformAdmin__Password", "s3cret-password");
        try
        {
            using var factory = new AuthApiFactory(_environment);
            using var scope = factory.Services.CreateScope();

            var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var admins = scope.ServiceProvider.GetRequiredService<IPlatformAdministratorRepository>();
            var user = await users.FindByEmailAsync(email, CancellationToken.None);

            Assert.NotNull(user);
            Assert.True(await admins.IsPlatformAdministratorAsync(user!.Id, CancellationToken.None));
        }
        finally
        {
            Environment.SetEnvironmentVariable("PlatformAdmin__Email", null);
            Environment.SetEnvironmentVariable("PlatformAdmin__Password", null);
        }
    }

    [Fact]
    public async Task Boot_WithoutPlatformAdminEnvVars_BootsNormallyWithoutSeeding()
    {
        Environment.SetEnvironmentVariable("PlatformAdmin__Email", null);
        Environment.SetEnvironmentVariable("PlatformAdmin__Password", null);

        using var factory = new AuthApiFactory(_environment);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/login");
        Assert.NotEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
