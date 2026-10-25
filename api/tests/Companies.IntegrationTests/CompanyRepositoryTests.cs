using Companies.Domain;
using Companies.Domain.Enums;
using Companies.Repositories;

namespace Companies.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class CompanyRepositoryTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public CompanyRepositoryTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task FindByPublicIdAsync_WithExistingCompany_ReturnsIt()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        var tenants = new TenantRepository(context);
        var (_, company, _) = await tenants.CreateWithFirstCompanyAsync(
            () => Tenant.Create("Acme"),
            tenantId => Company.Create(tenantId, "Acme Colombia", "CO"),
            companyId => Membership.Create(1, companyId, CompanyMembershipRole.Admin, DateTimeOffset.UtcNow),
            CancellationToken.None);

        var sut = new CompanyRepository(context);
        var found = await sut.FindByPublicIdAsync(company.PublicId, CancellationToken.None);

        Assert.NotNull(found);
        Assert.Equal(company.Id, found!.Id);
    }

    [Fact]
    public async Task FindByPublicIdAsync_WithUnknownPublicId_ReturnsNull()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        var sut = new CompanyRepository(context);
        var found = await sut.FindByPublicIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(found);
    }
}
