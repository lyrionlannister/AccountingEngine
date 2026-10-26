using Accounting.Domain;

namespace Accounting.Services;

public interface IAccountBalanceSnapshotRepository
{
    Task<AccountBalanceSnapshot?> FindByIdAsync(long companyId, long accountingPeriodId, long accountId, CancellationToken cancellationToken);

    Task<(decimal DebitTotal, decimal CreditTotal)> SumPostedLinesAsync(long companyId, long accountingPeriodId, long accountId, CancellationToken cancellationToken);

    Task AddAsync(AccountBalanceSnapshot snapshot, CancellationToken cancellationToken);

    Task UpdateAsync(AccountBalanceSnapshot snapshot, CancellationToken cancellationToken);
}
