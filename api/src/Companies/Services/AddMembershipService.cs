using Companies.Domain;
using Companies.Domain.Enums;
using Companies.DTOs;

namespace Companies.Services;

public sealed class AddMembershipService
{
    private readonly IMembershipRepository _memberships;

    public AddMembershipService(IMembershipRepository memberships) => _memberships = memberships;

    public async Task<AddMembershipResult> AddAsync(AddMembershipRequest request, CancellationToken cancellationToken)
    {
        var invokerMembership = await _memberships.FindActiveAsync(request.InvokerUserId, request.CompanyId, cancellationToken);
        if (invokerMembership is null || invokerMembership.Role != CompanyMembershipRole.Admin)
            throw new NotCompanyAdminException(request.InvokerUserId, request.CompanyId);

        var membership = Membership.Create(request.TargetUserId, request.CompanyId, request.Role, DateTimeOffset.UtcNow);
        await _memberships.AddAsync(membership, cancellationToken);

        return new AddMembershipResult(membership.Id, membership.PublicId, membership.UserId, membership.CompanyId, membership.Role);
    }
}
