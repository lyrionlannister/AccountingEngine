using Companies.DTOs;
using Companies.Repositories;
using Companies.Services;
using Microsoft.EntityFrameworkCore;

namespace Companies.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class RegisterUserServiceTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public RegisterUserServiceTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task RegisterAsync_WithNewEmail_CreatesUser()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        var sut = new RegisterUserService(new UserRepository(context));

        var result = await sut.RegisterAsync(new RegisterUserRequest("Someone@Example.com", "s3cret-password"), CancellationToken.None);

        Assert.NotEqual(0, result.Id);
        Assert.Equal("someone@example.com", result.Email);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmailDifferingOnlyByCase_ThrowsBeforeInserting()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        var sut = new RegisterUserService(new UserRepository(context));
        await sut.RegisterAsync(new RegisterUserRequest("someone@example.com", "s3cret-password"), CancellationToken.None);

        await Assert.ThrowsAsync<DuplicateEmailException>(
            () => sut.RegisterAsync(new RegisterUserRequest("SOMEONE@EXAMPLE.COM", "another-password"), CancellationToken.None));

        var count = await context.Users.CountAsync();
        Assert.Equal(1, count);
    }
}
