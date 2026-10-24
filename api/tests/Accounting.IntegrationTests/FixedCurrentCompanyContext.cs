using Shared;

namespace Accounting.IntegrationTests;

internal sealed class FixedCurrentCompanyContext(long companyId) : ICurrentCompanyContext
{
    public long CompanyId => companyId;
}
