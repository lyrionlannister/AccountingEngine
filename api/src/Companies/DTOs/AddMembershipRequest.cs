using Companies.Domain.Enums;

namespace Companies.DTOs;

public sealed record AddMembershipRequest(
    long InvokerUserId,
    long TargetUserId,
    long CompanyId,
    CompanyMembershipRole Role);

public sealed record AddMembershipResult(
    long Id,
    Guid PublicId,
    long UserId,
    long CompanyId,
    CompanyMembershipRole Role);
