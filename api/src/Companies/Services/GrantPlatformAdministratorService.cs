using Companies.Domain;
using Companies.DTOs;

namespace Companies.Services;

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
