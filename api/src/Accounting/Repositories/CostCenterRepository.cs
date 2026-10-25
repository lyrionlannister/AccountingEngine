using Accounting.Domain;
using Accounting.Persistence;
using Accounting.Services;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Repositories;

public sealed class CostCenterRepository : ICostCenterRepository
{
    private readonly AppDbContext _context;

    public CostCenterRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<CostCenter>> FindManyByIdAsync(long companyId, IReadOnlyCollection<long> ids, CancellationToken cancellationToken) =>
        await _context.CostCenters.Where(c => c.CompanyId == companyId && ids.Contains(c.Id)).ToListAsync(cancellationToken);
}
