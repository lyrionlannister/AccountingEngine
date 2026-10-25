using Accounting.Domain;
using Accounting.Persistence;
using Accounting.Services;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Repositories;

public sealed class AccountingPeriodRepository : IAccountingPeriodRepository
{
    private readonly AppDbContext _context;

    public AccountingPeriodRepository(AppDbContext context) => _context = context;

    public Task<AccountingPeriod?> FindByIdAsync(long companyId, long id, CancellationToken cancellationToken) =>
        _context.AccountingPeriods.FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Id == id, cancellationToken);

    public Task<AccountingPeriod?> FindByPublicIdAsync(long companyId, Guid publicId, CancellationToken cancellationToken) =>
        _context.AccountingPeriods.AsNoTracking().FirstOrDefaultAsync(p => p.CompanyId == companyId && p.PublicId == publicId, cancellationToken);

    public async Task UpdateAsync(AccountingPeriod period, CancellationToken cancellationToken) =>
        await _context.SaveChangesAsync(cancellationToken);
}
