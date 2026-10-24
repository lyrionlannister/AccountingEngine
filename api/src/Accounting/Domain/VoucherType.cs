using Accounting.Domain.Enums;

namespace Accounting.Domain;

public sealed class VoucherType
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long CompanyId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public VoucherNumberingMode NumberingMode { get; private set; }
    public bool IsActive { get; private set; }

    private VoucherType()
    {
    }

    public static VoucherType Create(long companyId, string code, string name, VoucherNumberingMode numberingMode)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return new VoucherType
        {
            CompanyId = companyId,
            Code = code,
            Name = name,
            NumberingMode = numberingMode,
            IsActive = true
        };
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
