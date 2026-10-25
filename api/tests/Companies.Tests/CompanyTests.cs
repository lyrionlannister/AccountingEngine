using Companies.Domain;

namespace Companies.Tests;

public class CompanyTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var company = Company.Create(1, "Acme Colombia", "CO");

        Assert.Equal(1, company.TenantId);
        Assert.Equal("Acme Colombia", company.Name);
        Assert.Equal("CO", company.CountryCode);
        Assert.True(company.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("C")]
    [InlineData("co")]
    [InlineData("COL")]
    public void Create_WithInvalidCountryCode_Throws(string countryCode)
    {
        Assert.Throws<ArgumentException>(() => Company.Create(1, "Acme", countryCode));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceName_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => Company.Create(1, name, "CO"));
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var company = Company.Create(1, "Acme", "MX");

        company.Deactivate();

        Assert.False(company.IsActive);
    }
}
