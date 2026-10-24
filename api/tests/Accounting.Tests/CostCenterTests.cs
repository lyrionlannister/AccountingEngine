using Accounting.Domain;

namespace Accounting.Tests;

public class CostCenterTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var costCenter = CostCenter.Create(1, "CC-01", "Ventas");

        Assert.Equal(1, costCenter.CompanyId);
        Assert.Equal("CC-01", costCenter.Code);
        Assert.Equal("Ventas", costCenter.Name);
        Assert.True(costCenter.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithoutCode_Throws(string code)
    {
        Assert.Throws<ArgumentException>(() => CostCenter.Create(1, code, "Ventas"));
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var costCenter = CostCenter.Create(1, "CC-01", "Ventas");

        costCenter.Deactivate();

        Assert.False(costCenter.IsActive);
    }
}
