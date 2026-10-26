namespace Auth.Domain;

public sealed class UserSession
{
    public static readonly TimeSpan SlidingWindow = TimeSpan.FromMinutes(30);

    public Guid SessionId { get; private set; }
    public long UserId { get; private set; }
    public string Ip { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }

    private UserSession()
    {
    }

    public static UserSession Create(long userId, string ip, DateTimeOffset createdAt)
    {
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId), "UserId must be positive.");
        if (string.IsNullOrWhiteSpace(ip))
            throw new ArgumentException("Ip is required.", nameof(ip));

        return new UserSession
        {
            SessionId = Guid.NewGuid(),
            UserId = userId,
            Ip = ip,
            CreatedAt = createdAt,
            LastSeenAt = createdAt
        };
    }

    public static UserSession Reconstitute(Guid sessionId, long userId, string ip, DateTimeOffset createdAt, DateTimeOffset lastSeenAt) =>
        new()
        {
            SessionId = sessionId,
            UserId = userId,
            Ip = ip,
            CreatedAt = createdAt,
            LastSeenAt = lastSeenAt
        };

    public void Touch(DateTimeOffset now) => LastSeenAt = now;

    public bool IsExpiredAt(DateTimeOffset now, TimeSpan slidingWindow) => now - LastSeenAt > slidingWindow;
}
