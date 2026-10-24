using Accounting.Domain;
using Accounting.Domain.Enums;

namespace Accounting.Tests;

public class VoucherTypeTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var voucherType = VoucherType.Create(1, "ING", "Ingreso", VoucherNumberingMode.Automatic);

        Assert.Equal(1, voucherType.CompanyId);
        Assert.Equal("ING", voucherType.Code);
        Assert.Equal("Ingreso", voucherType.Name);
        Assert.Equal(VoucherNumberingMode.Automatic, voucherType.NumberingMode);
        Assert.True(voucherType.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithoutCode_Throws(string code)
    {
        Assert.Throws<ArgumentException>(() => VoucherType.Create(1, code, "Ingreso", VoucherNumberingMode.Manual));
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var voucherType = VoucherType.Create(1, "ING", "Ingreso", VoucherNumberingMode.Manual);

        voucherType.Deactivate();

        Assert.False(voucherType.IsActive);
    }
}
