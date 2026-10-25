using Companies.Domain;
using Companies.Persistence;
using Companies.Services;
using Microsoft.EntityFrameworkCore;

namespace Companies.Repositories;

public sealed class CompanyRepository : ICompanyRepository
{
    private readonly CompaniesDbContext _context;

    public CompanyRepository(CompaniesDbContext context) => _context = context;

    public Task<Company?> FindByIdAsync(long id, CancellationToken cancellationToken) =>
        _context.Companies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Company?> FindByPublicIdAsync(Guid publicId, CancellationToken cancellationToken) =>
        _context.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.PublicId == publicId, cancellationToken);
}
