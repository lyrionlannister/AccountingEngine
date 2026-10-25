namespace Companies.Domain;

public sealed class PlatformAdministrator
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long UserId { get; private set; }
    public DateTimeOffset GrantedAt { get; private set; }

    private PlatformAdministrator()
    {
    }

    public static PlatformAdministrator Create(long userId, DateTimeOffset grantedAt)
    {
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId), "UserId must be positive.");

        return new PlatformAdministrator
        {
            UserId = userId,
            GrantedAt = grantedAt
        };
    }
}
