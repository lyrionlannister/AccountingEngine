namespace Users.Services;

// Único punto público que Auth puede llamar de Users para autenticar un login (ver
// tasks/plan-auth.md, Task 1). El hash y el motivo exacto del rechazo (email inexistente,
// contraseña incorrecta, usuario inactivo) nunca salen de este módulo — Auth solo recibe
// el UserId cuando las credenciales son válidas, o null en cualquier otro caso. Esto resuelve
// el mensaje genérico de login fallido (CLAUDE.md §11) en el punto donde es más barato hacerlo:
// aquí, no reconstruyéndolo del lado de Auth a partir de un hash que no debería ver.
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
