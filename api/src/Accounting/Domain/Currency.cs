namespace Accounting.Domain;

public sealed class Currency
{
    public long Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int DecimalPlaces { get; private set; }
    public bool IsActive { get; private set; }

    private Currency()
    {
    }

    public static Currency Create(string code, string name, int decimalPlaces)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 3 || !code.All(char.IsUpper))
            throw new ArgumentException("Code must be a 3-letter uppercase ISO 4217 code.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (decimalPlaces < 0)
            throw new ArgumentOutOfRangeException(nameof(decimalPlaces), "Decimal places cannot be negative.");

        return new Currency
        {
            Code = code,
            Name = name,
            DecimalPlaces = decimalPlaces,
            IsActive = true
        };
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
