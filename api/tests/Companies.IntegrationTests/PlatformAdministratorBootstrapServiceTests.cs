using Companies.Persistence;
using Companies.Repositories;
using Companies.Services;
using Microsoft.EntityFrameworkCore;
using Users.Persistence;
using Users.Repositories;
using Users.Services;

namespace Companies.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class PlatformAdministratorBootstrapServiceTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public PlatformAdministratorBootstrapServiceTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private PlatformAdministratorBootstrapService CreateSut(out IPlatformAdministratorRepository admins, out IUserRepository users, CompaniesDbContext companiesContext, UsersDbContext usersContext)
    {
        admins = new PlatformAdministratorRepository(companiesContext);
        users = new UserRepository(usersContext);
        return new PlatformAdministratorBootstrapService(admins, users, new RegisterUserService(users));
    }

    private UsersDbContext NewUsersContext() =>
        new(new DbContextOptionsBuilder<UsersDbContext>().UseNpgsql(_postgres).Options);

    [Fact]
    public async Task EnsureSeededAsync_NoExistingUserOrAdmin_CreatesUserAndPromotesIt()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var companiesContext = database.NewContext();
        await using var usersContext = NewUsersContext();
        var sut = CreateSut(out var admins, out var users, companiesContext, usersContext);

        await sut.EnsureSeededAsync("admin@acme.test", "s3cret-password", CancellationToken.None);

        var user = await users.FindByEmailAsync("admin@acme.test", CancellationToken.None);
        Assert.NotNull(user);
        Assert.True(await admins.IsPlatformAdministratorAsync(user!.Id, CancellationToken.None));
    }

    [Fact]
    public async Task EnsureSeededAsync_UserAlreadyExists_PromotesWithoutTouchingPassword()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var companiesContext = database.NewContext();
        await using var usersContext = NewUsersContext();
        var preexisting = await new RegisterUserService(new UserRepository(usersContext)).RegisterAsync(
            new Users.DTOs.RegisterUserRequest("admin@acme.test", "original-password"), CancellationToken.None);

        var sut = CreateSut(out var admins, out var users, companiesContext, usersContext);
        await sut.EnsureSeededAsync("admin@acme.test", "ignored-password", CancellationToken.None);

        Assert.True(await admins.IsPlatformAdministratorAsync(preexisting.Id, CancellationToken.None));
        var reloaded = await users.FindByEmailAsync("admin@acme.test", CancellationToken.None);
        Assert.Equal(preexisting.Id, reloaded!.Id);
    }

    [Fact]
    public async Task EnsureSeededAsync_CalledTwice_DoesNotCreateSecondAdminOrUser()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var companiesContext = database.NewContext();
        await using var usersContext = NewUsersContext();
        var sut = CreateSut(out var admins, out var users, companiesContext, usersContext);

        await sut.EnsureSeededAsync("admin@acme.test", "s3cret-password", CancellationToken.None);
        await sut.EnsureSeededAsync("admin@acme.test", "s3cret-password", CancellationToken.None);

        var user = await users.FindByEmailAsync("admin@acme.test", CancellationToken.None);
        Assert.NotNull(user);
    }

    [Fact]
    public async Task EnsureSeededAsync_NoExistingUserAndNoPassword_ThrowsInvalidOperationException()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var companiesContext = database.NewContext();
        await using var usersContext = NewUsersContext();
        var sut = CreateSut(out var admins, out var users, companiesContext, usersContext);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.EnsureSeededAsync("admin@acme.test", null, CancellationToken.None));

        Assert.Null(await users.FindByEmailAsync("admin@acme.test", CancellationToken.None));
    }

    [Fact]
    public async Task EnsureSeededAsync_AdminAlreadyExistsWithDifferentEmail_DoesNotPromoteNewEmail()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var companiesContext = database.NewContext();
        await using var usersContext = NewUsersContext();
        var sut = CreateSut(out var admins, out var users, companiesContext, usersContext);
        await sut.EnsureSeededAsync("first-admin@acme.test", "s3cret-password", CancellationToken.None);

        await sut.EnsureSeededAsync("second-admin@acme.test", "s3cret-password", CancellationToken.None);

        var secondUser = await users.FindByEmailAsync("second-admin@acme.test", CancellationToken.None);
        Assert.Null(secondUser);
    }
}
