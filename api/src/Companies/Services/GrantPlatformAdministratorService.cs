using Companies.Domain;
using Companies.DTOs;

namespace Companies.Services;

// El primer PlatformAdministrator no puede nacer de este Service (requeriría ya ser
// PlatformAdministrator para invocarlo). Se crea por seed de datos: ver
// src/Companies/Persistence/seed-first-platform-administrator.sql.
public sealed class GrantPlatformAdministratorService
{
    private readonly IPlatformAdministratorRepository _administrators;

    public GrantPlatformAdministratorService(IPlatformAdministratorRepository administrators) => _administrators = administrators;

    public async Task<GrantPlatformAdministratorResult> GrantAsync(
        GrantPlatformAdministratorRequest request, CancellationToken cancellationToken)
    {
        if (!await _administrators.IsPlatformAdministratorAsync(request.GrantedByUserId, cancellationToken))
            throw new NotPlatformAdministratorException(request.GrantedByUserId);

        var administrator = PlatformAdministrator.Create(request.TargetUserId, DateTimeOffset.UtcNow);
        await _administrators.AddAsync(administrator, cancellationToken);

        return new GrantPlatformAdministratorResult(administrator.Id, administrator.PublicId, administrator.UserId, administrator.GrantedAt);
    }
}
