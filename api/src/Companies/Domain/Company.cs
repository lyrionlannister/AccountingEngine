namespace Companies.Domain;

public sealed class Company
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private Company()
    {
    }

    public static Company Create(long tenantId, string name, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrEmpty(countryCode) || countryCode.Length != 2 || !countryCode.All(char.IsUpper))
            throw new ArgumentException("CountryCode must be a 2-letter uppercase ISO 3166-1 alpha-2 code.", nameof(countryCode));

        return new Company
        {
            TenantId = tenantId,
            Name = name,
            CountryCode = countryCode,
            IsActive = true
        };
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
