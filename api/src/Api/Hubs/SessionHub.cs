using System.IdentityModel.Tokens.Jwt;
using Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

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
