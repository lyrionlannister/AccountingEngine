using Accounting.Domain;
using Accounting.Persistence;
using Accounting.Services;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Repositories;

public sealed class PartyRepository : IPartyRepository
{
    private readonly AppDbContext _context;

    public PartyRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<Party>> FindManyByIdAsync(long companyId, IReadOnlyCollection<long> ids, CancellationToken cancellationToken) =>
        await _context.Parties.Where(p => p.CompanyId == companyId && ids.Contains(p.Id)).ToListAsync(cancellationToken);

    public Task<Party?> FindByPublicIdAsync(long companyId, Guid publicId, CancellationToken cancellationToken) =>
        _context.Parties.AsNoTracking().FirstOrDefaultAsync(p => p.CompanyId == companyId && p.PublicId == publicId, cancellationToken);
}
