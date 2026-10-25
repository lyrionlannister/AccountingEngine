using System.IdentityModel.Tokens.Jwt;
using Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

// Un solo grupo por UserId, resuelto del claim JWT al conectar — nunca de un parámetro que
// el cliente pueda falsear. SignalRSessionExpiryNotifier solo envía a ese grupo, nunca
// broadcast (ver tasks/plan-auth.md).
[Authorize]
public sealed class SessionHub : Hub
{
    public const string SessionExpiredEvent = "session_expired";

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (userId is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(userId));

        await base.OnConnectedAsync();
    }

    public static string GroupName(string userId) => $"user:{userId}";
}
