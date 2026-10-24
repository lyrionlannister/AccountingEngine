using Accounting.Domain;

namespace Accounting.Services;

public interface IVoucherTypeRepository
{
    Task<VoucherType?> FindByIdAsync(long companyId, long id, CancellationToken cancellationToken);
}
