using Companies.Domain;
using Companies.Domain.Enums;
using Companies.DTOs;
using Users.DTOs;
using Users.Services;

namespace Companies.Services;

public sealed class RegisterTenantWithOwnerService
{
    private readonly ITenantRepository _tenants;
    private readonly IUserRepositoryFactory _userRepositoryFactory;

    public RegisterTenantWithOwnerService(ITenantRepository tenants, IUserRepositoryFactory userRepositoryFactory)
    {
        _tenants = tenants;
        _userRepositoryFactory = userRepositoryFactory;
    }

    public async Task<RegisterTenantWithOwnerResult> RegisterAsync(
        RegisterTenantWithOwnerRequest request, CancellationToken cancellationToken)
    {
        var ownerUserId = 0L;

        var result = await _tenants.CreateWithOwnerAsync(
            createOwner: async (connection, transaction, ct) =>
            {
                var repository = _userRepositoryFactory.CreateFor(connection, transaction);
                var registered = await new RegisterUserService(repository)
                    .RegisterAsync(new RegisterUserRequest(request.Email, request.Password), ct);
                ownerUserId = registered.Id;
                return (registered.Id, registered.PublicId);
            },
            createTenant: () => Tenant.Create(request.TenantName),
            createCompany: tenantId => Company.Create(tenantId, request.CompanyName, request.CountryCode),
            createMembership: companyId => Membership.Create(ownerUserId, companyId, CompanyMembershipRole.Admin, DateTimeOffset.UtcNow),
            cancellationToken);

        return new RegisterTenantWithOwnerResult(
            result.OwnerPublicId, result.Tenant.PublicId, result.Company.PublicId, result.Membership.PublicId);
    }
}
