namespace Auth.Services;

public sealed class LogoutService
{
    private readonly ISessionStore _sessions;

    public LogoutService(ISessionStore sessions) => _sessions = sessions;

    public Task LogoutAsync(long userId, Guid sessionId, CancellationToken cancellationToken) =>
        _sessions.RevokeAsync(userId, sessionId, cancellationToken);
}
