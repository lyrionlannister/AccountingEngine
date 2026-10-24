namespace Companies.DTOs;

public sealed record CreateTenantWithFirstCompanyRequest(
    string TenantName,
    string CompanyName,
    string CountryCode,
    long CreatorUserId);
