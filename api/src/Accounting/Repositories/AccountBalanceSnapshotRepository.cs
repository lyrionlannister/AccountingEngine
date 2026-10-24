using Accounting.Domain;
using Accounting.Domain.Enums;
using Accounting.Persistence;
using Accounting.Services;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Repositories;

public sealed class AccountBalanceSnapshotRepository : IAccountBalanceSnapshotRepository
{
    private readonly AppDbContext _context;

    public AccountBalanceSnapshotRepository(AppDbContext context) => _context = context;

    public Task<AccountBalanceSnapshot?> FindByIdAsync(long accountingPeriodId, long accountId, CancellationToken cancellationToken) =>
        _context.AccountBalanceSnapshots
            .FirstOrDefaultAsync(s => s.AccountingPeriodId == accountingPeriodId && s.AccountId == accountId, cancellationToken);

    public async Task<(decimal DebitTotal, decimal CreditTotal)> SumPostedLinesAsync(long companyId, long accountingPeriodId, long accountId, CancellationToken cancellationToken)
    {
        var lines = await _context.JournalEntries
            .Where(e => e.CompanyId == companyId && e.AccountingPeriodId == accountingPeriodId && e.Status == JournalEntryStatus.Posted)
            .SelectMany(e => e.Lines)
            .Where(l => l.AccountId == accountId)
            .Select(l => new { l.DebitAmount, l.CreditAmount })
            .ToListAsync(cancellationToken);

        return (lines.Sum(l => l.DebitAmount), lines.Sum(l => l.CreditAmount));
    }

    public async Task AddAsync(AccountBalanceSnapshot snapshot, CancellationToken cancellationToken)
    {
        _context.AccountBalanceSnapshots.Add(snapshot);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AccountBalanceSnapshot snapshot, CancellationToken cancellationToken) =>
        await _context.SaveChangesAsync(cancellationToken);
}
