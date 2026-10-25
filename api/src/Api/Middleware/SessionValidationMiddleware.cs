using System.IdentityModel.Tokens.Jwt;
using Api.Security;
using Auth.Services;

namespace Api.Middleware;

// Un JWT con firma válida no basta: su "sid" debe seguir existiendo en ISessionStore
// (logout/expiración lo revocan). También refresca el TTL deslizante de la sesión en cada
// request autenticado — ver tasks/plan-auth.md.
public sealed class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionValidationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ISessionStore sessions)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var sub = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var sid = context.User.FindFirst(SessionClaims.SessionId)?.Value;

            if (!long.TryParse(sub, out var userId) || !Guid.TryParse(sid, out var sessionId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var session = await sessions.FindAsync(userId, sessionId, context.RequestAborted);
            if (session is null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            await sessions.TouchAsync(userId, sessionId, context.RequestAborted);
        }

        await _next(context);
    }
}
