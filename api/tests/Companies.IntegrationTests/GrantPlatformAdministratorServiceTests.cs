using Companies.Domain;
using Companies.DTOs;
using Companies.Repositories;
using Companies.Services;
using Microsoft.EntityFrameworkCore;
using Users.DTOs;
using Users.Persistence;
using Users.Repositories;
using Users.Services;

namespace Companies.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class GrantPlatformAdministratorServiceTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public GrantPlatformAdministratorServiceTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GrantAsync_InvokerNotPlatformAdministrator_Throws()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        var sut = new GrantPlatformAdministratorService(new PlatformAdministratorRepository(context));

        await Assert.ThrowsAsync<NotPlatformAdministratorException>(() => sut.GrantAsync(
            new GrantPlatformAdministratorRequest(GrantedByUserId: 1, TargetUserId: 2), CancellationToken.None));
    }

    [Fact]
    public async Task GrantAsync_InvokerIsPlatformAdministrator_GrantsNewAdministrator()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        var usersOptions = new DbContextOptionsBuilder<UsersDbContext>().UseNpgsql(_postgres).Options;
        await using var usersContext = new UsersDbContext(usersOptions);
        var users = new UserRepository(usersContext);
        var grantedBy = await new RegisterUserService(users).RegisterAsync(
            new RegisterUserRequest("admin@acme.test", "s3cret-password"), CancellationToken.None);
        var target = await new RegisterUserService(users).RegisterAsync(
            new RegisterUserRequest("target@acme.test", "s3cret-password"), CancellationToken.None);

        var administrators = new PlatformAdministratorRepository(context);
        await administrators.AddAsync(PlatformAdministrator.Create(grantedBy.Id, DateTimeOffset.UtcNow), CancellationToken.None);

        var sut = new GrantPlatformAdministratorService(administrators);

        var result = await sut.GrantAsync(
            new GrantPlatformAdministratorRequest(grantedBy.Id, target.Id), CancellationToken.None);

        Assert.Equal(target.Id, result.UserId);
    }
}
