using Accounting.Domain;

namespace Accounting.Tests;

public class PartyTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var party = Party.Create(1, "P-01", "Acme Corp");

        Assert.Equal(1, party.CompanyId);
        Assert.Equal("P-01", party.Code);
        Assert.Equal("Acme Corp", party.Name);
        Assert.True(party.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithoutCode_Throws(string code)
    {
        Assert.Throws<ArgumentException>(() => Party.Create(1, code, "Acme Corp"));
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var party = Party.Create(1, "P-01", "Acme Corp");

        party.Deactivate();

        Assert.False(party.IsActive);
    }
}
