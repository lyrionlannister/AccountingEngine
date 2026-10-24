using Companies.Domain;

namespace Companies.Tests;

public class PlatformAdministratorTests
{
    [Fact]
    public void Create_WithValidUserId_SetsProperties()
    {
        var grantedAt = DateTimeOffset.UtcNow;

        var administrator = PlatformAdministrator.Create(1, grantedAt);

        Assert.Equal(1, administrator.UserId);
        Assert.Equal(grantedAt, administrator.GrantedAt);
    }

    [Fact]
    public void Create_WithNonPositiveUserId_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PlatformAdministrator.Create(0, DateTimeOffset.UtcNow));
    }
}
