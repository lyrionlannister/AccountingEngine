using Accounting.Domain;
using Accounting.Persistence;
using Accounting.Services;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<Account>> FindManyByIdAsync(long companyId, IReadOnlyCollection<long> ids, CancellationToken cancellationToken) =>
        await _context.Accounts.Where(a => a.CompanyId == companyId && ids.Contains(a.Id)).ToListAsync(cancellationToken);
}
