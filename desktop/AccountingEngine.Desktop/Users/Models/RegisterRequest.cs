namespace AccountingEngine.Desktop.Users.Models;

public sealed record RegisterRequest(string Email, string Password, string TenantName, string CompanyName, string CountryCode);
