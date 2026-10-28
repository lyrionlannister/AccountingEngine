using Companies.DTOs;
using Companies.Persistence;
using Companies.Repositories;
using Companies.Services;
using Microsoft.EntityFrameworkCore;
using Users.Persistence;
using Users.Repositories;

namespace Companies.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class RegisterTenantWithOwnerServiceTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public RegisterTenantWithOwnerServiceTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private RegisterTenantWithOwnerService CreateSut(CompaniesDbContext companiesContext) =>
        new(new TenantRepository(companiesContext), new UserRepositoryFactory());

    [Fact]
    public async Task RegisterAsync_NewEmail_CreatesOwnerTenantCompanyAndAdminMembership()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var companiesContext = database.NewContext();
        var sut = CreateSut(companiesContext);

        var result = await sut.RegisterAsync(
            new RegisterTenantWithOwnerRequest("owner@acme.test", "s3cret-password", "Acme", "Acme MX", "MX"),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.OwnerPublicId);
        Assert.NotEqual(Guid.Empty, result.TenantPublicId);
        Assert.NotEqual(Guid.Empty, result.CompanyPublicId);
        Assert.NotEqual(Guid.Empty, result.MembershipPublicId);

        await using var usersContext = new UsersDbContext(
            new DbContextOptionsBuilder<UsersDbContext>().UseNpgsql(_postgres).Options);
        var user = await usersContext.Users.FirstOrDefaultAsync(u => u.Email == "owner@acme.test");
        Assert.NotNull(user);
        Assert.Equal(result.OwnerPublicId, user!.PublicId);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_PropagatesWithoutCreatingTenantOrCompany()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var firstContext = database.NewContext();
        await CreateSut(firstContext).RegisterAsync(
            new RegisterTenantWithOwnerRequest("duplicate@acme.test", "s3cret-password", "Acme", "Acme MX", "MX"),
            CancellationToken.None);

        await using var secondContext = database.NewContext();
        await Assert.ThrowsAsync<Users.Services.DuplicateEmailException>(() => CreateSut(secondContext).RegisterAsync(
            new RegisterTenantWithOwnerRequest("duplicate@acme.test", "another-password", "Other", "Other MX", "MX"),
            CancellationToken.None));

        await using var verify = database.NewContext();
        Assert.Equal(1, await verify.Tenants.CountAsync());
        Assert.Equal(1, await verify.Companies.CountAsync());
    }
}
