namespace Companies.DTOs;

public sealed record GrantPlatformAdministratorRequest(long GrantedByUserId, long TargetUserId);

public sealed record GrantPlatformAdministratorResult(long Id, Guid PublicId, long UserId, DateTimeOffset GrantedAt);
