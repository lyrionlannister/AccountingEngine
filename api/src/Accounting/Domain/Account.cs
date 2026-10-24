using Accounting.Domain.Enums;

namespace Accounting.Domain;

public sealed class Account
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long CompanyId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public AccountNature Nature { get; private set; }
    public AccountType Type { get; private set; }
    public long? ParentAccountId { get; private set; }
    public bool AllowsPosting { get; private set; }
    public bool RequiresCostCenter { get; private set; }
    public bool RequiresParty { get; private set; }
    public bool IsActive { get; private set; }

    private Account()
    {
    }

    public static Account Create(
        long companyId,
        string code,
        string name,
        AccountNature nature,
        AccountType type,
        long? parentAccountId,
        bool allowsPosting)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return new Account
        {
            CompanyId = companyId,
            Code = code,
            Name = name,
            Nature = nature,
            Type = type,
            ParentAccountId = parentAccountId,
            AllowsPosting = allowsPosting,
            IsActive = true
        };
    }

    public void SetParent(long? parentAccountId)
    {
        if (parentAccountId.HasValue && Id != 0 && parentAccountId.Value == Id)
            throw new InvalidOperationException("An account cannot be its own parent.");

        ParentAccountId = parentAccountId;
    }

    public void SetPostingRequirements(bool requiresCostCenter, bool requiresParty)
    {
        RequiresCostCenter = requiresCostCenter;
        RequiresParty = requiresParty;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
