using Companies.Domain;

namespace Companies.Services;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(long id, CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);

    Task AddAsync(User user, CancellationToken cancellationToken);
}
