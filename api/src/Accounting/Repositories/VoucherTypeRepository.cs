using Accounting.Domain;
using Accounting.Persistence;
using Accounting.Services;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Repositories;

public sealed class VoucherTypeRepository : IVoucherTypeRepository
{
    private readonly AppDbContext _context;

    public VoucherTypeRepository(AppDbContext context) => _context = context;

    public Task<VoucherType?> FindByIdAsync(long companyId, long id, CancellationToken cancellationToken) =>
        _context.VoucherTypes.FirstOrDefaultAsync(v => v.CompanyId == companyId && v.Id == id, cancellationToken);

    public Task<VoucherType?> FindByPublicIdAsync(long companyId, Guid publicId, CancellationToken cancellationToken) =>
        _context.VoucherTypes.AsNoTracking().FirstOrDefaultAsync(v => v.CompanyId == companyId && v.PublicId == publicId, cancellationToken);
}
