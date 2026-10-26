using Accounting.Domain;
using Accounting.Persistence;
using Accounting.Services;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Repositories;

public sealed class CurrencyRepository : ICurrencyRepository
{
    private readonly AppDbContext _context;

    public CurrencyRepository(AppDbContext context) => _context = context;

    public Task<Currency?> FindByCodeAsync(string code, CancellationToken cancellationToken) =>
        _context.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Code == code, cancellationToken);

    public async Task<IReadOnlyList<Currency>> FindManyByCodeAsync(IReadOnlyCollection<string> codes, CancellationToken cancellationToken) =>
        await _context.Currencies.AsNoTracking().Where(c => codes.Contains(c.Code)).ToListAsync(cancellationToken);
}
