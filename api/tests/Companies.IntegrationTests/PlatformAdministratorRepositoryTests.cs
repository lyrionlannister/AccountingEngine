using Companies.Domain;
using Companies.Repositories;

namespace Companies.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class PlatformAdministratorRepositoryTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public PlatformAdministratorRepositoryTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ExistsAnyAsync_NoAdministrators_ReturnsFalse()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();
        var sut = new PlatformAdministratorRepository(context);

        var result = await sut.ExistsAnyAsync(CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsAnyAsync_WithAtLeastOneAdministrator_ReturnsTrue()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();
        var sut = new PlatformAdministratorRepository(context);
        await sut.AddAsync(PlatformAdministrator.Create(1, DateTimeOffset.UtcNow), CancellationToken.None);

        var result = await sut.ExistsAnyAsync(CancellationToken.None);

        Assert.True(result);
    }
}
