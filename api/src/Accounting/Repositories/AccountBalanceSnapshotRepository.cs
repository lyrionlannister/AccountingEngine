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

    public Task<AccountBalanceSnapshot?> FindByIdAsync(long companyId, long accountingPeriodId, long accountId, CancellationToken cancellationToken) =>
        _context.AccountBalanceSnapshots
            .FirstOrDefaultAsync(s => s.CompanyId == companyId && s.AccountingPeriodId == accountingPeriodId && s.AccountId == accountId, cancellationToken);

    public async Task<(decimal DebitTotal, decimal CreditTotal)> SumPostedLinesAsync(long companyId, long accountingPeriodId, long accountId, CancellationToken cancellationToken)
    {
        var totals = await _context.JournalEntries
            .Where(e => e.CompanyId == companyId && e.AccountingPeriodId == accountingPeriodId && e.Status == JournalEntryStatus.Posted)
            .SelectMany(e => e.Lines)
            .Where(l => l.AccountId == accountId)
            .GroupBy(_ => 1)
            .Select(g => new { Debit = g.Sum(l => l.DebitAmount), Credit = g.Sum(l => l.CreditAmount) })
            .FirstOrDefaultAsync(cancellationToken);

        return totals is null ? (0m, 0m) : (totals.Debit, totals.Credit);
    }

    public async Task AddAsync(AccountBalanceSnapshot snapshot, CancellationToken cancellationToken)
    {
        _context.AccountBalanceSnapshots.Add(snapshot);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AccountBalanceSnapshot snapshot, CancellationToken cancellationToken) =>
        await _context.SaveChangesAsync(cancellationToken);
}
