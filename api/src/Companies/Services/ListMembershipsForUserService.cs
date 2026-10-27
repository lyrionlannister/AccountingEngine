using Companies.DTOs;

namespace Companies.Services;

public sealed class ListMembershipsForUserService
{
    private readonly IMembershipRepository _memberships;
    private readonly ICompanyRepository _companies;

    public ListMembershipsForUserService(IMembershipRepository memberships, ICompanyRepository companies)
    {
        _memberships = memberships;
        _companies = companies;
    }

    public async Task<IReadOnlyList<MembershipSummary>> ListActiveAsync(long userId, CancellationToken cancellationToken)
    {
        var memberships = await _memberships.ListActiveByUserAsync(userId, cancellationToken);
        if (memberships.Count == 0)
            return [];

        var companyIds = memberships.Select(m => m.CompanyId).Distinct().ToList();
        var companies = await _companies.FindByIdsAsync(companyIds, cancellationToken);
        var companiesById = companies.ToDictionary(c => c.Id);

        return memberships
            .Select(m => new MembershipSummary(companiesById[m.CompanyId].PublicId, companiesById[m.CompanyId].Name, m.Role))
            .ToList();
    }
}
