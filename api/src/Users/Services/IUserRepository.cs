using Users.Domain;

namespace Users.Services;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(long id, CancellationToken cancellationToken);

    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);

    Task AddAsync(User user, CancellationToken cancellationToken);
}
