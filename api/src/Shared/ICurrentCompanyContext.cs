namespace Shared;

public interface ICurrentCompanyContext
{
    long CompanyId { get; }

    // String, no el enum de Companies.Domain: Shared no depende de ningun modulo (CLAUDE.md
    // §3.9). Cada modulo que necesite autorizar por rol interpreta el valor el mismo (hoy:
    // "Admin" | "Accountant" | "Viewer", los nombres de Companies.Domain.Enums.CompanyMembershipRole).
    string Role { get; }
}
