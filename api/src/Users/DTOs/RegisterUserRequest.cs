namespace Users.DTOs;

public sealed record RegisterUserRequest(string Email, string Password);

public sealed record RegisterUserResult(long Id, Guid PublicId, string Email);
