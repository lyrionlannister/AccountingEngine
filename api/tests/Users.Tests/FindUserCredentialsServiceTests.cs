using Users.Domain;
using Users.Services;

namespace Users.Tests;

public class FindUserCredentialsServiceTests
{
    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly User? _user;

        public FakeUserRepository(User? user) => _user = user;

        public Task<User?> FindByIdAsync(long id, CancellationToken cancellationToken) => Task.FromResult(_user);

        public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken) => Task.FromResult(_user);

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken) => Task.FromResult(_user is not null);

        public Task AddAsync(User user, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    [Fact]
    public async Task VerifyCredentialsAsync_WithCorrectPassword_ReturnsUserId()
    {
        var user = User.Create("someone@example.com", PasswordHasher.Hash("correct-password"));
        var sut = new FindUserCredentialsService(new FakeUserRepository(user));

        var result = await sut.VerifyCredentialsAsync("someone@example.com", "correct-password", CancellationToken.None);

        Assert.Equal(user.Id, result);
    }

    [Fact]
    public async Task VerifyCredentialsAsync_WithWrongPassword_ReturnsNull()
    {
        var user = User.Create("someone@example.com", PasswordHasher.Hash("correct-password"));
        var sut = new FindUserCredentialsService(new FakeUserRepository(user));

        var result = await sut.VerifyCredentialsAsync("someone@example.com", "wrong-password", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task VerifyCredentialsAsync_WithUnknownEmail_ReturnsNull()
    {
        var sut = new FindUserCredentialsService(new FakeUserRepository(null));

        var result = await sut.VerifyCredentialsAsync("nobody@example.com", "any-password", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task VerifyCredentialsAsync_WithInactiveUser_ReturnsNullEvenWithCorrectPassword()
    {
        var user = User.Create("someone@example.com", PasswordHasher.Hash("correct-password"));
        user.Deactivate();
        var sut = new FindUserCredentialsService(new FakeUserRepository(user));

        var result = await sut.VerifyCredentialsAsync("someone@example.com", "correct-password", CancellationToken.None);

        Assert.Null(result);
    }
}
