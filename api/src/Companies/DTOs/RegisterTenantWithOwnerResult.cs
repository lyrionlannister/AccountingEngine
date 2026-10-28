namespace Companies.DTOs;

public sealed record RegisterTenantWithOwnerResult(
    Guid OwnerPublicId,
    Guid TenantPublicId,
    Guid CompanyPublicId,
    Guid MembershipPublicId);
