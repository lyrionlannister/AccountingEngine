using Companies.Domain;
using Companies.Domain.Enums;
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
public sealed class CurrentCompanyContextTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public CurrentCompanyContextTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ResolveAsync_WithoutActiveMembership_ThrowsNotFound()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        var sut = new CurrentCompanyContext(new MembershipRepository(context));

        await Assert.ThrowsAsync<CompanyMembershipNotFoundException>(
            () => sut.ResolveAsync(userId: 999, companyId: 999, CancellationToken.None));
    }

    [Fact]
    public async Task CompanyId_BeforeResolve_Throws()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        var sut = new CurrentCompanyContext(new MembershipRepository(context));

        Assert.Throws<InvalidOperationException>(() => sut.CompanyId);
    }

    [Fact]
    public async Task ResolveAsync_WithActiveMembership_ExposesCompanyId()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        var usersOptions = new DbContextOptionsBuilder<UsersDbContext>().UseNpgsql(_postgres).Options;
        await using var usersContext = new UsersDbContext(usersOptions);
        var founder = await new RegisterUserService(new UserRepository(usersContext)).RegisterAsync(
            new RegisterUserRequest("founder@acme.test", "s3cret-password"), CancellationToken.None);

        var tenants = new TenantRepository(context);
        var (_, company, membership) = await tenants.CreateWithFirstCompanyAsync(
            () => Tenant.Create("Acme"),
            tenantId => Company.Create(tenantId, "Acme Colombia", "CO"),
            companyId => Membership.Create(founder.Id, companyId, CompanyMembershipRole.Admin, DateTimeOffset.UtcNow),
            CancellationToken.None);

        var sut = new CurrentCompanyContext(new MembershipRepository(context));
        await sut.ResolveAsync(membership.UserId, company.Id, CancellationToken.None);

        Assert.Equal(company.Id, sut.CompanyId);
    }
}
