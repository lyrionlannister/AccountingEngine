namespace Users.Services;

public sealed class FindUserCredentialsService
{
    private readonly IUserRepository _users;

    public FindUserCredentialsService(IUserRepository users) => _users = users;

    public async Task<long?> VerifyCredentialsAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await _users.FindByEmailAsync(email, cancellationToken);
        if (user is null || !user.IsActive)
            return null;

        return PasswordHasher.Verify(password, user.PasswordHash) ? user.Id : null;
    }
}
