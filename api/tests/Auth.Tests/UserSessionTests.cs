using Auth.Domain;

namespace Auth.Tests;

public class UserSessionTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var session = UserSession.Create(userId: 1, ip: "203.0.113.10", Now);

        Assert.NotEqual(Guid.Empty, session.SessionId);
        Assert.Equal(1, session.UserId);
        Assert.Equal("203.0.113.10", session.Ip);
        Assert.Equal(Now, session.CreatedAt);
        Assert.Equal(Now, session.LastSeenAt);
    }

    [Fact]
    public void Create_WithInvalidUserId_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => UserSession.Create(userId: 0, "203.0.113.10", Now));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithMissingIp_Throws(string ip)
    {
        Assert.Throws<ArgumentException>(() => UserSession.Create(userId: 1, ip, Now));
    }

    [Fact]
    public void Touch_UpdatesLastSeenAt_WithoutChangingCreatedAt()
    {
        var session = UserSession.Create(userId: 1, "203.0.113.10", Now);
        var later = Now.AddMinutes(10);

        session.Touch(later);

        Assert.Equal(Now, session.CreatedAt);
        Assert.Equal(later, session.LastSeenAt);
    }

    [Fact]
    public void IsExpiredAt_WithinSlidingWindow_ReturnsFalse()
    {
        var session = UserSession.Create(userId: 1, "203.0.113.10", Now);

        Assert.False(session.IsExpiredAt(Now.AddMinutes(29), TimeSpan.FromMinutes(30)));
    }

    [Fact]
    public void IsExpiredAt_PastSlidingWindow_ReturnsTrue()
    {
        var session = UserSession.Create(userId: 1, "203.0.113.10", Now);

        Assert.True(session.IsExpiredAt(Now.AddMinutes(31), TimeSpan.FromMinutes(30)));
    }

    [Fact]
    public void Reconstitute_PreservesTheGivenSessionId()
    {
        var sessionId = Guid.NewGuid();
        var later = Now.AddMinutes(5);

        var session = UserSession.Reconstitute(sessionId, userId: 1, "203.0.113.10", Now, later);

        Assert.Equal(sessionId, session.SessionId);
        Assert.Equal(1, session.UserId);
        Assert.Equal("203.0.113.10", session.Ip);
        Assert.Equal(Now, session.CreatedAt);
        Assert.Equal(later, session.LastSeenAt);
    }
}
