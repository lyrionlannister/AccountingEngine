using Auth.Services;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

public sealed class SignalRSessionExpiryNotifier : ISessionExpiryNotifier
{
    private readonly IHubContext<SessionHub> _hub;

    public SignalRSessionExpiryNotifier(IHubContext<SessionHub> hub) => _hub = hub;

    public Task NotifyExpiredAsync(long userId, Guid sessionId, CancellationToken cancellationToken) =>
        _hub.Clients.Group(SessionHub.GroupName(userId.ToString()))
            .SendAsync(SessionHub.SessionExpiredEvent, new { sessionId }, cancellationToken);
}
