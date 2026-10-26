using Shared;

namespace Companies.Services;

// No hay capa HTTP todavía: ResolveAsync recibe userId/companyId como parámetros explícitos
// en lugar de leerlos de un token o de la sesión.
public sealed class CurrentCompanyContext : ICurrentCompanyContext
{
    private readonly IMembershipRepository _memberships;
    private long? _companyId;
    private string? _role;

    public CurrentCompanyContext(IMembershipRepository memberships) => _memberships = memberships;

    public long CompanyId => _companyId
        ?? throw new InvalidOperationException("Company context has not been resolved. Call ResolveAsync first.");

    public string Role => _role
        ?? throw new InvalidOperationException("Company context has not been resolved. Call ResolveAsync first.");

    public async Task ResolveAsync(long userId, long companyId, CancellationToken cancellationToken)
    {
        var membership = await _memberships.FindActiveAsync(userId, companyId, cancellationToken)
            ?? throw new CompanyMembershipNotFoundException(userId, companyId);

        _companyId = membership.CompanyId;
        _role = membership.Role.ToString();
    }
}
