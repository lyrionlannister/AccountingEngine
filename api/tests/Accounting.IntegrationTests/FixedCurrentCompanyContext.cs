using Shared;

namespace Accounting.IntegrationTests;

internal sealed class FixedCurrentCompanyContext(long companyId, string role = "Admin") : ICurrentCompanyContext
{
    public long CompanyId => companyId;
    public string Role => role;
}
