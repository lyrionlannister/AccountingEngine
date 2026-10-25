using Microsoft.EntityFrameworkCore;
using Users.Domain;
using Users.Persistence;
using Users.Services;

namespace Users.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly UsersDbContext _context;

    public UserRepository(UsersDbContext context) => _context = context;

    public Task<User?> FindByIdAsync(long id, CancellationToken cancellationToken) =>
        _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        _context.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLowerInvariant(), cancellationToken);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken) =>
        _context.Users.AnyAsync(u => u.Email == email.Trim().ToLowerInvariant(), cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
