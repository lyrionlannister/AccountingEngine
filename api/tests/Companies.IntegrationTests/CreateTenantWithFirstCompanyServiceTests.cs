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
public sealed class CreateTenantWithFirstCompanyServiceTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public CreateTenantWithFirstCompanyServiceTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateAsync_CreatesTenantCompanyAndAdminMembership()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        var usersOptions = new DbContextOptionsBuilder<UsersDbContext>().UseNpgsql(_postgres).Options;
        await using var usersContext = new UsersDbContext(usersOptions);
        var users = new UserRepository(usersContext);
        var user = await new RegisterUserService(users).RegisterAsync(
            new RegisterUserRequest("founder@acme.test", "s3cret-password"), CancellationToken.None);

        var sut = new CreateTenantWithFirstCompanyService(new TenantRepository(context));

        var result = await sut.CreateAsync(
            new CreateTenantWithFirstCompanyRequest("Acme Holdings", "Acme Colombia", "CO", user.Id), CancellationToken.None);

        Assert.NotEqual(0, result.TenantId);
        Assert.NotEqual(0, result.CompanyId);
        Assert.NotEqual(0, result.MembershipId);
    }

    [Fact]
    public async Task CreateAsync_WhenMembershipStepFails_RollsBackTenantAndCompany()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        var sut = new CreateTenantWithFirstCompanyService(new TenantRepository(context));

        // CreatorUserId = 0 hace que Membership.Create lance ArgumentOutOfRangeException
        // después de que Tenant y Company ya se insertaron dentro de la misma transacción.
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.CreateAsync(
            new CreateTenantWithFirstCompanyRequest("Acme Holdings", "Acme Colombia", "CO", CreatorUserId: 0),
            CancellationToken.None));

        Assert.Equal(0, await context.Tenants.CountAsync());
        Assert.Equal(0, await context.Companies.CountAsync());
    }
}
