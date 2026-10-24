using Companies.Domain;
using Companies.DTOs;

namespace Companies.Services;

public sealed class RegisterUserService
{
    private readonly IUserRepository _users;

    public RegisterUserService(IUserRepository users) => _users = users;

    public async Task<RegisterUserResult> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        if (await _users.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new DuplicateEmailException(request.Email);

        var passwordHash = PasswordHasher.Hash(request.Password);
        var user = User.Create(request.Email, passwordHash);

        await _users.AddAsync(user, cancellationToken);

        return new RegisterUserResult(user.Id, user.PublicId, user.Email);
    }
}
