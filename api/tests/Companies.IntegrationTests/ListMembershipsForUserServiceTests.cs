using Companies.Domain;
using Companies.Domain.Enums;
using Companies.Repositories;
using Companies.Services;

namespace Companies.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class ListMembershipsForUserServiceTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public ListMembershipsForUserServiceTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ListActiveAsync_ReturnsOnlyActiveMembershipsForThatUser()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        var tenants = new TenantRepository(context);
        var (_, companyA, membershipA) = await tenants.CreateWithFirstCompanyAsync(
            () => Tenant.Create("Acme"),
            tenantId => Company.Create(tenantId, "Acme Colombia", "CO"),
            companyId => Membership.Create(userId: 1, companyId, CompanyMembershipRole.Admin, DateTimeOffset.UtcNow),
            CancellationToken.None);
        var (_, companyB, _) = await tenants.CreateWithFirstCompanyAsync(
            () => Tenant.Create("Acme MX"),
            tenantId => Company.Create(tenantId, "Acme Mexico", "MX"),
            companyId => Membership.Create(userId: 2, companyId, CompanyMembershipRole.Admin, DateTimeOffset.UtcNow),
            CancellationToken.None);

        var memberships = new MembershipRepository(context);
        var revoked = Membership.Create(userId: 1, companyB.Id, CompanyMembershipRole.Viewer, DateTimeOffset.UtcNow);
        revoked.Revoke();
        await memberships.AddAsync(revoked, CancellationToken.None);

        var sut = new ListMembershipsForUserService(memberships);

        var result = await sut.ListActiveAsync(userId: 1, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(companyA.Id, result[0].CompanyId);
        Assert.Equal(CompanyMembershipRole.Admin, result[0].Role);
        Assert.Equal(membershipA.CompanyId, result[0].CompanyId);
    }

    [Fact]
    public async Task ListActiveAsync_WithNoMemberships_ReturnsEmpty()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        var sut = new ListMembershipsForUserService(new MembershipRepository(context));

        var result = await sut.ListActiveAsync(userId: 999, CancellationToken.None);

        Assert.Empty(result);
    }
}
