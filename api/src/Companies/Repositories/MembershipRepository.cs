using Companies.Domain;
using Companies.Persistence;
using Companies.Services;
using Microsoft.EntityFrameworkCore;

namespace Companies.Repositories;

public sealed class MembershipRepository : IMembershipRepository
{
    private readonly CompaniesDbContext _context;

    public MembershipRepository(CompaniesDbContext context) => _context = context;

    public Task<Membership?> FindActiveAsync(long userId, long companyId, CancellationToken cancellationToken) =>
        _context.Memberships.FirstOrDefaultAsync(
            m => m.UserId == userId && m.CompanyId == companyId && m.IsActive, cancellationToken);

    public async Task<IReadOnlyList<Membership>> ListActiveByUserAsync(long userId, CancellationToken cancellationToken) =>
        await _context.Memberships.Where(m => m.UserId == userId && m.IsActive).ToListAsync(cancellationToken);

    public async Task AddAsync(Membership membership, CancellationToken cancellationToken)
    {
        _context.Memberships.Add(membership);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
