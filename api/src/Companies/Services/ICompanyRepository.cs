using Companies.Domain;

namespace Companies.Services;

public interface ICompanyRepository
{
    Task<Company?> FindByIdAsync(long id, CancellationToken cancellationToken);

    Task<Company?> FindByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
}
