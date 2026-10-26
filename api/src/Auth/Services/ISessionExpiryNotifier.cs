namespace Auth.Services;

public interface ISessionExpiryNotifier
{
    Task NotifyExpiredAsync(long userId, Guid sessionId, CancellationToken cancellationToken);
}
