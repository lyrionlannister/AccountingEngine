namespace Companies.Services;

// Mensaje deliberadamente genérico: nunca distingue "la membresía no existe" de
// "existe pero no pertenece a este usuario" (404 en vez de 403, ver skill tenant-isolation).
public sealed class CompanyMembershipNotFoundException : Exception
{
    public CompanyMembershipNotFoundException(long userId, long companyId)
        : base($"No active membership found for user {userId} in company {companyId}.")
    {
    }
}

public sealed class NotCompanyAdminException : Exception
{
    public NotCompanyAdminException(long userId, long companyId)
        : base($"User {userId} does not have an Admin membership in company {companyId}.")
    {
    }
}

public sealed class NotPlatformAdministratorException : Exception
{
    public NotPlatformAdministratorException(long userId)
        : base($"User {userId} is not a platform administrator.")
    {
    }
}
