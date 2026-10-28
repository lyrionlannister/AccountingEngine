using Companies.Domain;
using Users.DTOs;
using Users.Services;

namespace Companies.Services;

public sealed class PlatformAdministratorBootstrapService
{
    private readonly IPlatformAdministratorRepository _admins;
    private readonly IUserRepository _users;
    private readonly RegisterUserService _register;

    public PlatformAdministratorBootstrapService(
        IPlatformAdministratorRepository admins, IUserRepository users, RegisterUserService register)
    {
        _admins = admins;
        _users = users;
        _register = register;
    }

    public async Task EnsureSeededAsync(string email, string? password, CancellationToken cancellationToken)
    {
        if (await _admins.ExistsAnyAsync(cancellationToken))
            return;

        var user = await _users.FindByEmailAsync(email, cancellationToken);
        if (user is null && string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException(
                $"No se puede crear el primer Platform Administrator ('{email}') sin contraseña. " +
                "Define PlatformAdmin:Password (env var PlatformAdmin__Password).");

        var userId = user?.Id ?? (await _register.RegisterAsync(new RegisterUserRequest(email, password!), cancellationToken)).Id;

        await _admins.AddAsync(PlatformAdministrator.Create(userId, DateTimeOffset.UtcNow), cancellationToken);
    }
}
