using Companies.Domain;

namespace Companies.Tests;

public class TenantTests
{
    [Fact]
    public void Create_WithValidName_SetsProperties()
    {
        var tenant = Tenant.Create("Acme Holdings");

        Assert.Equal("Acme Holdings", tenant.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceName_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => Tenant.Create(name));
    }
}
