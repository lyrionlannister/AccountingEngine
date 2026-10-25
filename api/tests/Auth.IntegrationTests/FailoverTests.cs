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

// Prueba el escenario central de tasks/plan-auth.md: Redis es la fuente de verdad del camino
// caliente, pero si deja de responder, FailoverSessionStore sigue sirviendo contra Postgres
// sin lanzar al usuario. Esta clase apaga el contenedor de Redis de la colección compartida;
// como xUnit corre las pruebas de una misma collection en serie, es seguro, siempre que se
// reinicie Redis al salir (DisposeAsync).
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

        // Un IConnectionMultiplexer ya conectado detecta la caída en la siguiente operación
        // (ConnectTimeout corto) en vez de fallar al construirlo — se crea aquí, después de
        // apagar Redis, para no esperar su timeout de conexión inicial en cada prueba.
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

        // Redis vuelve sano pero nunca tuvo esta sesión — solo existe en el respaldo de
        // Postgres. Sin el fallback-on-miss en FailoverSessionStore.FindAsync, esto devolvía
        // null (sesión "perdida") en cuanto Redis volvía a responder.
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
