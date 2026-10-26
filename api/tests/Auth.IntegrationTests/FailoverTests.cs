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
public sealed class FailoverTests : IAsyncLifetime
{
    private readonly AuthTestEnvironment _env;
    private string _postgres = null!;

    public FailoverTests(AuthTestEnvironment env) => _env = env;

    public async Task InitializeAsync() => _postgres = await _env.CreatePostgresDatabaseAsync();

    public async Task DisposeAsync() => await _env.StartRedisAsync();

    [Fact]
    public async Task CreateAsync_WhenRedisIsDown_FallsBackToPostgresWithoutThrowing()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);

        await using var usersContext = database.NewUsersContext();
        await new RegisterUserService(new UserRepository(usersContext)).RegisterAsync(
            new RegisterUserRequest("failover@acme.test", "s3cret-password"), CancellationToken.None);

        await _env.StopRedisAsync();

        using var redis = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
        {
            EndPoints = { _env.RedisConnectionString },
            ConnectTimeout = 500,
            AbortOnConnectFail = false
        });

        var sut = new LoginService(
            new FindUserCredentialsService(new UserRepository(database.NewUsersContext())),
            new FailoverSessionStore(
                new RedisSessionStore(redis),
                new PostgresSessionFallbackStore(database.NewAuthContext()),
                NullLogger<FailoverSessionStore>.Instance),
            new ListMembershipsForUserService(new MembershipRepository(database.NewCompaniesContext())));

        var result = await sut.LoginAsync(
            new LoginRequest("failover@acme.test", "s3cret-password", "203.0.113.10"), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.SessionId);

        await using var authContext = database.NewAuthContext();
        var persisted = await new PostgresSessionFallbackStore(authContext)
            .FindAsync(result.UserId, result.SessionId, CancellationToken.None);
        Assert.NotNull(persisted);
    }

    [Fact]
    public async Task FindAsync_AfterRedisRecoversFromOutage_StillFindsASessionCreatedDuringTheOutage()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);

        await using var usersContext = database.NewUsersContext();
        await new RegisterUserService(new UserRepository(usersContext)).RegisterAsync(
            new RegisterUserRequest("failover-recovery@acme.test", "s3cret-password"), CancellationToken.None);

        await _env.StopRedisAsync();

        using var redisDuringOutage = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
        {
            EndPoints = { _env.RedisConnectionString },
            ConnectTimeout = 500,
            AbortOnConnectFail = false
        });

        var storeDuringOutage = new FailoverSessionStore(
            new RedisSessionStore(redisDuringOutage),
            new PostgresSessionFallbackStore(database.NewAuthContext()),
            NullLogger<FailoverSessionStore>.Instance);

        var sut = new LoginService(
            new FindUserCredentialsService(new UserRepository(database.NewUsersContext())),
            storeDuringOutage,
            new ListMembershipsForUserService(new MembershipRepository(database.NewCompaniesContext())));

        var result = await sut.LoginAsync(
            new LoginRequest("failover-recovery@acme.test", "s3cret-password", "203.0.113.10"), CancellationToken.None);

        await _env.StartRedisAsync();

        using var redisAfterRecovery = await ConnectionMultiplexer.ConnectAsync(_env.RedisConnectionString);
        var storeAfterRecovery = new FailoverSessionStore(
            new RedisSessionStore(redisAfterRecovery),
            new PostgresSessionFallbackStore(database.NewAuthContext()),
            NullLogger<FailoverSessionStore>.Instance);

        var found = await storeAfterRecovery.FindAsync(result.UserId, result.SessionId, CancellationToken.None);

        Assert.NotNull(found);
        Assert.Equal(result.SessionId, found!.SessionId);
    }
}
