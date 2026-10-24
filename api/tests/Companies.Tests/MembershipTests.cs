using Companies.Domain;
using Companies.Domain.Enums;

namespace Companies.Tests;

public class MembershipTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var grantedAt = DateTimeOffset.UtcNow;

        var membership = Membership.Create(1, 2, CompanyMembershipRole.Admin, grantedAt);

        Assert.Equal(1, membership.UserId);
        Assert.Equal(2, membership.CompanyId);
        Assert.Equal(CompanyMembershipRole.Admin, membership.Role);
        Assert.Equal(grantedAt, membership.GrantedAt);
        Assert.True(membership.IsActive);
    }

    [Fact]
    public void Create_WithNonPositiveUserId_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Membership.Create(0, 1, CompanyMembershipRole.Viewer, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Create_WithNonPositiveCompanyId_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Membership.Create(1, 0, CompanyMembershipRole.Viewer, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Revoke_SetsIsActiveFalse()
    {
        var membership = Membership.Create(1, 2, CompanyMembershipRole.Admin, DateTimeOffset.UtcNow);

        membership.Revoke();

        Assert.False(membership.IsActive);
    }
}
