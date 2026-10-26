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

    public async Task<AccountingPeriod> CloseAsync(
        long companyId, long accountingPeriodId, Func<AccountingPeriod, CancellationToken, Task> closeAction, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        // SQL crudo a proposito, mismo motivo que JournalEntryRepository.LockAndRequirePeriodOpenAsync:
        // FOR UPDATE serializa el cierre contra un posteo concurrente sobre el mismo periodo.
        var period = await _context.AccountingPeriods
            .FromSqlInterpolated($"SELECT * FROM periodos_contables WHERE id = {accountingPeriodId} AND empresa_id = {companyId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Accounting period {accountingPeriodId} not found.");

        await closeAction(period, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return period;
    }
}
