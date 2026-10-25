using Companies.DTOs;

namespace Companies.Services;

// Único punto público que Auth puede llamar de Companies tras un login exitoso, para saber
// a qué empresas pertenece el usuario (ver tasks/plan-auth.md, Task 2).
public sealed class ListMembershipsForUserService
{
    private readonly IMembershipRepository _memberships;

    public ListMembershipsForUserService(IMembershipRepository memberships) => _memberships = memberships;

    public async Task<IReadOnlyList<MembershipSummary>> ListActiveAsync(long userId, CancellationToken cancellationToken)
    {
        var memberships = await _memberships.ListActiveByUserAsync(userId, cancellationToken);

        return memberships.Select(m => new MembershipSummary(m.CompanyId, m.Role)).ToList();
    }
}
