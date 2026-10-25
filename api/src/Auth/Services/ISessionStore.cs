using Auth.Domain;

namespace Auth.Services;

public interface ISessionStore
{
    Task<UserSession> CreateAsync(long userId, string ip, CancellationToken cancellationToken);

    Task<UserSession?> FindAsync(Guid sessionId, CancellationToken cancellationToken);

    Task TouchAsync(Guid sessionId, CancellationToken cancellationToken);

    Task RevokeAsync(Guid sessionId, CancellationToken cancellationToken);
}
