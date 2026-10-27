using Companies.Domain;

namespace Companies.Services;

public interface IPlatformAdministratorRepository
{
    Task<bool> IsPlatformAdministratorAsync(long userId, CancellationToken cancellationToken);

    Task<bool> ExistsAnyAsync(CancellationToken cancellationToken);

    Task AddAsync(PlatformAdministrator administrator, CancellationToken cancellationToken);
}
