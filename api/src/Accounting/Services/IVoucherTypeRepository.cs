using Accounting.Domain;

namespace Accounting.Services;

public interface IVoucherTypeRepository
{
    Task<VoucherType?> FindByIdAsync(long companyId, long id, CancellationToken cancellationToken);

    Task<VoucherType?> FindByPublicIdAsync(long companyId, Guid publicId, CancellationToken cancellationToken);
}
