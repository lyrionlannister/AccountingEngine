using Auth.DTOs;
using Auth.Repositories;
using Auth.Services;
using Companies.Repositories;
using Companies.Services;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using Users.DTOs;
using Users.Repositories;
using Users.Services;

namespace Auth.IntegrationTests;

[Collection(AuthTestCollection.Name)]
public sealed class LoginConcurrencyTests : IAsyncLifetime
{
    private readonly AuthTestEnvironment _env;
    private string _postgres = null!;
    private IConnectionMultiplexer _redis = null!;

    public LoginConcurrencyTests(AuthTestEnvironment env) => _env = env;

    public async Task InitializeAsync()
    {
        _postgres = await _env.CreatePostgresDatabaseAsync();
        _redis = await ConnectionMultiplexer.ConnectAsync(_env.RedisConnectionString);
    }

    public Task DisposeAsync()
    {
        _redis.Dispose();
        return Task.CompletedTask;
    }

    private async Task RegisterUserAsync(TestDatabase database, string email)
    {
        await using var usersContext = database.NewUsersContext();
        await new RegisterUserService(new UserRepository(usersContext)).RegisterAsync(
            new RegisterUserRequest(email, "s3cret-password"), CancellationToken.None);
    }

    private LoginService BuildLoginService(TestDatabase database)
    {
        var sessionStore = new FailoverSessionStore(
            new RedisSessionStore(_redis),
            new PostgresSessionFallbackStore(database.NewAuthContext()),
            NullLogger<FailoverSessionStore>.Instance);

        return new LoginService(
            new FindUserCredentialsService(new UserRepository(database.NewUsersContext())),
            sessionStore,
            new ListMembershipsForUserService(new MembershipRepository(database.NewCompaniesContext())));
    }

    [Fact]
    public async Task LoginAsync_ConcurrentFromDifferentIps_OnlyOneSucceeds()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await RegisterUserAsync(database, "concurrent-diff-ip@acme.test");

        var sut = BuildLoginService(database);

        var first = await sut.LoginAsync(
            new LoginRequest("concurrent-diff-ip@acme.test", "s3cret-password", "203.0.113.10"), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, first.SessionId);

        await Assert.ThrowsAsync<SessionConflictException>(() => sut.LoginAsync(
            new LoginRequest("concurrent-diff-ip@acme.test", "s3cret-password", "198.51.100.20"), CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_ConcurrentFromSameIp_BothSucceed()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await RegisterUserAsync(database, "concurrent-same-ip@acme.test");

        var sut = BuildLoginService(database);

        var first = await sut.LoginAsync(
            new LoginRequest("concurrent-same-ip@acme.test", "s3cret-password", "203.0.113.10"), CancellationToken.None);
        var second = await sut.LoginAsync(
            new LoginRequest("concurrent-same-ip@acme.test", "s3cret-password", "203.0.113.10"), CancellationToken.None);

        Assert.NotEqual(first.SessionId, second.SessionId);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsInvalidCredentials()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await RegisterUserAsync(database, "wrong-password@acme.test");

        var sut = BuildLoginService(database);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => sut.LoginAsync(
            new LoginRequest("wrong-password@acme.test", "not-the-password", "203.0.113.10"), CancellationToken.None));
    }

    [Fact]
    public async Task TouchAsync_ExtendsSessionLifetimeInRedis()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await RegisterUserAsync(database, "touch@acme.test");

        var sut = BuildLoginService(database);
        var login = await sut.LoginAsync(new LoginRequest("touch@acme.test", "s3cret-password", "203.0.113.10"), CancellationToken.None);

        var redisStore = new RedisSessionStore(_redis);
        var before = await redisStore.FindAsync(login.UserId, login.SessionId, CancellationToken.None);
        Assert.NotNull(before);

        await Task.Delay(1100);
        await redisStore.TouchAsync(login.UserId, login.SessionId, CancellationToken.None);

        var after = await redisStore.FindAsync(login.UserId, login.SessionId, CancellationToken.None);
        Assert.NotNull(after);
        Assert.True(after!.LastSeenAt > before!.LastSeenAt);
    }
}
