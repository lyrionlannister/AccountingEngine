using Companies.Domain;
using Companies.Domain.Enums;
using Companies.DTOs;
using Companies.Persistence;
using Companies.Repositories;
using Companies.Services;
using Microsoft.EntityFrameworkCore;
using Users.DTOs;
using Users.Persistence;
using Users.Repositories;
using Users.Services;

namespace Companies.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class AddMembershipAuthorizationTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public AddMembershipAuthorizationTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<RegisterUserResult> RegisterUserAsync(string email)
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>().UseNpgsql(_postgres).Options;
        await using var usersContext = new UsersDbContext(options);

        return await new RegisterUserService(new UserRepository(usersContext)).RegisterAsync(
            new RegisterUserRequest(email, "s3cret-password"), CancellationToken.None);
    }

    [Fact]
    public async Task AddAsync_InvokerWithoutAdminMembership_ThrowsNotCompanyAdmin()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        var founder = await RegisterUserAsync("founder@acme.test");
        var viewer = await RegisterUserAsync("viewer@acme.test");
        var target = await RegisterUserAsync("target@acme.test");

        var tenants = new TenantRepository(context);
        var (_, company, _) = await tenants.CreateWithFirstCompanyAsync(
            () => Tenant.Create("Acme"),
            tenantId => Company.Create(tenantId, "Acme Colombia", "CO"),
            companyId => Membership.Create(founder.Id, companyId, CompanyMembershipRole.Admin, DateTimeOffset.UtcNow),
            CancellationToken.None);

        var memberships = new MembershipRepository(context);
        await memberships.AddAsync(
            Membership.Create(viewer.Id, company.Id, CompanyMembershipRole.Viewer, DateTimeOffset.UtcNow), CancellationToken.None);

        var sut = new AddMembershipService(memberships);

        await Assert.ThrowsAsync<NotCompanyAdminException>(() => sut.AddAsync(
            new AddMembershipRequest(viewer.Id, target.Id, company.Id, CompanyMembershipRole.Viewer),
            CancellationToken.None));
    }

    [Fact]
    public async Task AddAsync_InvokerWithAdminMembership_CreatesMembership()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        var founder = await RegisterUserAsync("founder@acme.test");
        var target = await RegisterUserAsync("target@acme.test");

        var tenants = new TenantRepository(context);
        var (_, company, adminMembership) = await tenants.CreateWithFirstCompanyAsync(
            () => Tenant.Create("Acme"),
            tenantId => Company.Create(tenantId, "Acme Colombia", "CO"),
            companyId => Membership.Create(founder.Id, companyId, CompanyMembershipRole.Admin, DateTimeOffset.UtcNow),
            CancellationToken.None);

        var memberships = new MembershipRepository(context);
        var sut = new AddMembershipService(memberships);

        var result = await sut.AddAsync(
            new AddMembershipRequest(adminMembership.UserId, target.Id, company.Id, CompanyMembershipRole.Accountant),
            CancellationToken.None);

        Assert.Equal(target.Id, result.UserId);
        Assert.Equal(CompanyMembershipRole.Accountant, result.Role);
    }

    [Fact]
    public async Task AddAsync_DuplicateUserCompanyPair_FailsWithoutSilentlyDuplicating()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        var founder = await RegisterUserAsync("founder@acme.test");
        var target = await RegisterUserAsync("target@acme.test");

        var tenants = new TenantRepository(context);
        var (_, company, adminMembership) = await tenants.CreateWithFirstCompanyAsync(
            () => Tenant.Create("Acme"),
            tenantId => Company.Create(tenantId, "Acme Colombia", "CO"),
            companyId => Membership.Create(founder.Id, companyId, CompanyMembershipRole.Admin, DateTimeOffset.UtcNow),
            CancellationToken.None);

        var memberships = new MembershipRepository(context);
        var sut = new AddMembershipService(memberships);

        await sut.AddAsync(
            new AddMembershipRequest(adminMembership.UserId, target.Id, company.Id, CompanyMembershipRole.Viewer),
            CancellationToken.None);

        await Assert.ThrowsAnyAsync<Exception>(() => sut.AddAsync(
            new AddMembershipRequest(adminMembership.UserId, target.Id, company.Id, CompanyMembershipRole.Admin),
            CancellationToken.None));
    }
}
