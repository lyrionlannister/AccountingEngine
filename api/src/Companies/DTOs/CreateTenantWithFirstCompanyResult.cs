namespace Companies.DTOs;

public sealed record CreateTenantWithFirstCompanyResult(
    long TenantId,
    Guid TenantPublicId,
    long CompanyId,
    Guid CompanyPublicId,
    long MembershipId,
    Guid MembershipPublicId);
