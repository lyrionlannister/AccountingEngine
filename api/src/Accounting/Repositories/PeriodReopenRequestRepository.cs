using Accounting.Domain;
using Accounting.Persistence;
using Accounting.Services;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Repositories;

public sealed class PeriodReopenRequestRepository : IPeriodReopenRequestRepository
{
    private readonly AppDbContext _context;

    public PeriodReopenRequestRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(PeriodReopenRequest request, CancellationToken cancellationToken)
    {
        _context.PeriodReopenRequests.Add(request);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<PeriodReopenRequest?> FindByIdAsync(long companyId, long id, CancellationToken cancellationToken) =>
        _context.PeriodReopenRequests.FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Id == id, cancellationToken);

    public async Task UpdateAsync(PeriodReopenRequest request, CancellationToken cancellationToken) =>
        await _context.SaveChangesAsync(cancellationToken);
}
