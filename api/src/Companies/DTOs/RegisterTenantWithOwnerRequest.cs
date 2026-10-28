namespace Companies.DTOs;

public sealed record RegisterTenantWithOwnerRequest(
    string Email,
    string Password,
    string TenantName,
    string CompanyName,
    string CountryCode);
