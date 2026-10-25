using Auth.Repositories;

namespace Auth.Tests;

public class RedisSessionExpiryListenerTests
{
    [Fact]
    public void TryParseSessionKey_WithValidSessionKey_ParsesUserIdAndSessionId()
    {
        var sessionId = Guid.NewGuid();

        var result = RedisSessionExpiryListener.TryParseSessionKey($"session:42:{sessionId}", out var userId, out var parsedSessionId);

        Assert.True(result);
        Assert.Equal(42, userId);
        Assert.Equal(sessionId, parsedSessionId);
    }

    [Theory]
    [InlineData("user_sessions:42")]
    [InlineData("report_cache:some-key")]
    [InlineData("session:not-a-number:also-not-a-guid")]
    [InlineData("session:42")]
    public void TryParseSessionKey_WithNonSessionOrMalformedKey_ReturnsFalse(string key)
    {
        var result = RedisSessionExpiryListener.TryParseSessionKey(key, out _, out _);

        Assert.False(result);
    }
}
