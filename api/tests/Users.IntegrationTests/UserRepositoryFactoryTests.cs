using Npgsql;
using Users.DTOs;
using Users.Repositories;
using Users.Services;

namespace Users.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class UserRepositoryFactoryTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public UserRepositoryFactoryTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateFor_UsesTheGivenConnectionAndTransaction_AndDoesNotCloseItOnDispose()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);

        await using var connection = new NpgsqlConnection(_postgres);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var sut = new UserRepositoryFactory();
        var repository = sut.CreateFor(connection, transaction);
        var registered = await new RegisterUserService(repository)
            .RegisterAsync(new RegisterUserRequest("shared-tx@example.com", "s3cret-password"), CancellationToken.None);

        Assert.Equal(System.Data.ConnectionState.Open, connection.State);

        await transaction.CommitAsync();

        await using var verifyContext = database.NewContext();
        var reloaded = await verifyContext.Users.FindAsync(registered.Id);
        Assert.NotNull(reloaded);
    }
}
