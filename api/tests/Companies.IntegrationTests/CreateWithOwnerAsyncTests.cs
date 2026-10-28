using Companies.Domain;
using Companies.Domain.Enums;
using Companies.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Companies.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class CreateWithOwnerAsyncTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public CreateWithOwnerAsyncTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateWithOwnerAsync_HappyPath_CreatesOwnerTenantCompanyAndMembership()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();
        var sut = new TenantRepository(context);

        var result = await sut.CreateWithOwnerAsync(
            createOwner: (_, _, _) => Task.FromResult((UserId: 42L, UserPublicId: Guid.NewGuid())),
            createTenant: () => Tenant.Create("Acme"),
            createCompany: tenantId => Company.Create(tenantId, "Acme MX", "MX"),
            createMembership: companyId => Membership.Create(42, companyId, CompanyMembershipRole.Admin, DateTimeOffset.UtcNow),
            cancellationToken: CancellationToken.None);

        Assert.Equal(42, result.OwnerUserId);
        Assert.Equal(result.Tenant.Id, result.Company.TenantId);
        Assert.Equal(result.Company.Id, result.Membership.CompanyId);
        Assert.Equal(42, result.Membership.UserId);
    }

    [Fact]
    public async Task CreateWithOwnerAsync_CreateOwnerThrows_CreatesNothing()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();
        var sut = new TenantRepository(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateWithOwnerAsync(
            createOwner: (_, _, _) => throw new InvalidOperationException("boom"),
            createTenant: () => Tenant.Create("Acme"),
            createCompany: tenantId => Company.Create(tenantId, "Acme MX", "MX"),
            createMembership: companyId => Membership.Create(42, companyId, CompanyMembershipRole.Admin, DateTimeOffset.UtcNow),
            cancellationToken: CancellationToken.None));

        await using var verify = database.NewContext();
        Assert.Equal(0, await verify.Tenants.CountAsync());
        Assert.Equal(0, await verify.Companies.CountAsync());
        Assert.Equal(0, await verify.Memberships.CountAsync());
    }

    [Fact]
    public async Task CreateWithOwnerAsync_CreateMembershipThrows_RollsBackOwnerToo()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();
        var sut = new TenantRepository(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateWithOwnerAsync(
            createOwner: (_, _, _) => Task.FromResult((UserId: 42L, UserPublicId: Guid.NewGuid())),
            createTenant: () => Tenant.Create("Acme"),
            createCompany: tenantId => Company.Create(tenantId, "Acme MX", "MX"),
            createMembership: _ => throw new InvalidOperationException("boom"),
            cancellationToken: CancellationToken.None));

        await using var verify = database.NewContext();
        Assert.Equal(0, await verify.Tenants.CountAsync());
        Assert.Equal(0, await verify.Companies.CountAsync());
    }
}
