using Companies.Domain.Enums;

namespace Companies.Domain;

public sealed class Membership
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long UserId { get; private set; }
    public long CompanyId { get; private set; }
    public CompanyMembershipRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset GrantedAt { get; private set; }

    private Membership()
    {
    }

    public static Membership Create(long userId, long companyId, CompanyMembershipRole role, DateTimeOffset grantedAt)
    {
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId), "UserId must be positive.");
        if (companyId <= 0)
            throw new ArgumentOutOfRangeException(nameof(companyId), "CompanyId must be positive.");

        return new Membership
        {
            UserId = userId,
            CompanyId = companyId,
            Role = role,
            IsActive = true,
            GrantedAt = grantedAt
        };
    }

    public void Revoke() => IsActive = false;
}
