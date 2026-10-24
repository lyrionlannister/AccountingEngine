namespace Companies.Domain;

public sealed class Tenant
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Tenant()
    {
    }

    public static Tenant Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return new Tenant { Name = name };
    }
}
