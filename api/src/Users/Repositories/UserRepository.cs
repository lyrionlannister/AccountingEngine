using Microsoft.EntityFrameworkCore;
using Npgsql;
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

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "ix_usuarios_correo_normalizado"
        })
        {
            // El chequeo previo (ExistsByEmailAsync en RegisterUserService) no basta bajo
            // concurrencia: dos registros con el mismo email pueden pasarlo ambos antes de que
            // cualquiera inserte. El índice único es el backstop real (mismo patrón que
            // JournalEntryRepository con el conflicto de idempotencia).
            throw new DuplicateEmailException(user.Email);
        }
    }
}
