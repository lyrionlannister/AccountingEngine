using Companies.Domain;
using Companies.Persistence;
using Companies.Services;
using Microsoft.EntityFrameworkCore;

namespace Companies.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly CompaniesDbContext _context;

    public UserRepository(CompaniesDbContext context) => _context = context;

    public Task<User?> FindByIdAsync(long id, CancellationToken cancellationToken) =>
        _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken) =>
        _context.Users.AnyAsync(u => u.Email == email.Trim().ToLowerInvariant(), cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
