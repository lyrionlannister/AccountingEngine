using Companies.Domain;

namespace Companies.Tests;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var user = User.Create("Someone@Example.com", "hashed-value");

        Assert.Equal("someone@example.com", user.Email);
        Assert.Equal("hashed-value", user.PasswordHash);
        Assert.True(user.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    [InlineData("@missing-local.com")]
    public void Create_WithInvalidEmail_Throws(string email)
    {
        Assert.Throws<ArgumentException>(() => User.Create(email, "hashed-value"));
    }

    [Fact]
    public void Create_WithMissingPasswordHash_Throws()
    {
        Assert.Throws<ArgumentException>(() => User.Create("someone@example.com", ""));
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var user = User.Create("someone@example.com", "hashed-value");

        user.Deactivate();

        Assert.False(user.IsActive);
    }
}
