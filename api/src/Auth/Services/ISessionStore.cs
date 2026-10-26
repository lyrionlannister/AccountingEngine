using Auth.Domain;

namespace Auth.Services;

public interface ISessionStore
{
    Task<UserSession> CreateAsync(long userId, string ip, CancellationToken cancellationToken);

    Task<UserSession?> FindAsync(long userId, Guid sessionId, CancellationToken cancellationToken);

    Task TouchAsync(long userId, Guid sessionId, CancellationToken cancellationToken);

    Task RevokeAsync(long userId, Guid sessionId, CancellationToken cancellationToken);
}
