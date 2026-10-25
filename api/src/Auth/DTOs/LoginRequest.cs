using Companies.DTOs;

namespace Auth.DTOs;

public sealed record LoginRequest(string Email, string Password, string Ip);

public sealed record LoginResult(long UserId, Guid SessionId, IReadOnlyList<MembershipSummary> Memberships);
