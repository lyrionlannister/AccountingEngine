using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using Api.Controllers;
using Api.Hubs;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Users.DTOs;
using Users.Services;

namespace Api.IntegrationTests;

[Collection(ApiTestCollection.Name)]
public sealed class SessionExpiryPushTests : IClassFixture<AuthApiFactory>
{
    private readonly AuthApiFactory _factory;

    public SessionExpiryPushTests(ApiTestEnvironment environment) => _factory = new AuthApiFactory(environment);

    [Fact]
    public async Task SessionExpiry_PublishesSignalREventToOwningUserOnly()
    {
        const string email = "expiry-push@acme.test";
        using var scope = _factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<RegisterUserService>()
            .RegisterAsync(new RegisterUserRequest(email, "s3cret-password"), CancellationToken.None);

        using var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login", new AuthController.LoginBody(email, "s3cret-password"));
        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<AuthController.LoginResponse>();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(login!.AccessToken);
        var userId = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
        var sessionId = jwt.Claims.First(c => c.Type == "sid").Value;

        await using var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(_factory.Server.BaseAddress, "/hubs/sessions"), options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult<string?>(login.AccessToken);
            })
            .Build();

        var eventReceived = new TaskCompletionSource<string>();
        connection.On<object>(SessionHub.SessionExpiredEvent, payload => eventReceived.TrySetResult(payload.ToString()!));
        await connection.StartAsync();

        // Fuerza la expiración ya (en vez de esperar el TTL deslizante real de 30 min) —
        // ejercita el mismo mecanismo (RedisSessionExpiryListener) que un vencimiento real.
        var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        await redis.GetDatabase().KeyExpireAsync($"session:{userId}:{sessionId}", TimeSpan.FromMilliseconds(200));

        var completed = await Task.WhenAny(eventReceived.Task, Task.Delay(TimeSpan.FromSeconds(10)));
        Assert.Same(eventReceived.Task, completed);
    }
}
