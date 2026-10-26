namespace Auth.Services;

public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("Invalid credentials.")
    {
    }
}

public sealed class SessionConflictException : Exception
{
    public long UserId { get; }
    public string RejectedIp { get; }

    public SessionConflictException(long userId, string rejectedIp)
        : base($"User {userId} already has an active session from a different IP; rejected login from {rejectedIp}.")
    {
        UserId = userId;
        RejectedIp = rejectedIp;
    }
}
