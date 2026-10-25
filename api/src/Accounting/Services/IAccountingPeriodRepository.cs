using Accounting.Domain;

namespace Accounting.Services;

public interface IAccountingPeriodRepository
{
    Task<AccountingPeriod?> FindByIdAsync(long companyId, long id, CancellationToken cancellationToken);

    Task<AccountingPeriod?> FindByPublicIdAsync(long companyId, Guid publicId, CancellationToken cancellationToken);

    Task UpdateAsync(AccountingPeriod period, CancellationToken cancellationToken);
}
