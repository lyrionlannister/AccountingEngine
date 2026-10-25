using Companies.Domain;
using Companies.Domain.Enums;
using Companies.DTOs;

namespace Companies.Services;

public sealed class CreateTenantWithFirstCompanyService
{
    private readonly ITenantRepository _tenants;

    public CreateTenantWithFirstCompanyService(ITenantRepository tenants) => _tenants = tenants;

    public async Task<CreateTenantWithFirstCompanyResult> CreateAsync(
        CreateTenantWithFirstCompanyRequest request, CancellationToken cancellationToken)
    {
        var (tenant, company, membership) = await _tenants.CreateWithFirstCompanyAsync(
            () => Tenant.Create(request.TenantName),
            tenantId => Company.Create(tenantId, request.CompanyName, request.CountryCode),
            companyId => Membership.Create(request.CreatorUserId, companyId, CompanyMembershipRole.Admin, DateTimeOffset.UtcNow),
            cancellationToken);

        return new CreateTenantWithFirstCompanyResult(
            tenant.Id, tenant.PublicId, company.Id, company.PublicId, membership.Id, membership.PublicId);
    }
}
