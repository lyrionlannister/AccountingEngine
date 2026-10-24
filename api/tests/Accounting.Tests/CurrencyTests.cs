using Accounting.Domain;

namespace Accounting.Tests;

public class CurrencyTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var currency = Currency.Create("MXN", "Peso mexicano", 2);

        Assert.Equal("MXN", currency.Code);
        Assert.Equal("Peso mexicano", currency.Name);
        Assert.Equal(2, currency.DecimalPlaces);
        Assert.True(currency.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("MX")]
    [InlineData("mxn")]
    [InlineData("MXNN")]
    public void Create_WithInvalidCode_Throws(string code)
    {
        Assert.Throws<ArgumentException>(() => Currency.Create(code, "Peso", 2));
    }

    [Fact]
    public void Create_WithNegativeDecimalPlaces_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Currency.Create("MXN", "Peso", -1));
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var currency = Currency.Create("MXN", "Peso mexicano", 2);

        currency.Deactivate();

        Assert.False(currency.IsActive);
    }
}
