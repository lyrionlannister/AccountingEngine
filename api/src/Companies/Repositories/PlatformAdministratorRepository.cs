using Companies.Domain;
using Companies.Persistence;
using Companies.Services;
using Microsoft.EntityFrameworkCore;

namespace Companies.Repositories;

public sealed class PlatformAdministratorRepository : IPlatformAdministratorRepository
{
    private readonly CompaniesDbContext _context;

    public PlatformAdministratorRepository(CompaniesDbContext context) => _context = context;

    public Task<bool> IsPlatformAdministratorAsync(long userId, CancellationToken cancellationToken) =>
        _context.PlatformAdministrators.AnyAsync(a => a.UserId == userId, cancellationToken);

    public Task<bool> ExistsAnyAsync(CancellationToken cancellationToken) =>
        _context.PlatformAdministrators.AnyAsync(cancellationToken);

    public async Task AddAsync(PlatformAdministrator administrator, CancellationToken cancellationToken)
    {
        _context.PlatformAdministrators.Add(administrator);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
