using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Api.Security;
using Auth.DTOs;
using Auth.Services;
using Companies.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly LoginService _login;
    private readonly LogoutService _logout;
    private readonly JwtTokenIssuer _tokens;

    public AuthController(LoginService login, LogoutService logout, JwtTokenIssuer tokens)
    {
        _login = login;
        _logout = logout;
        _tokens = tokens;
    }

    public sealed record LoginBody(string Email, string Password);

    public sealed record LoginResponse(string AccessToken, IReadOnlyList<MembershipSummary> Memberships);

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginBody body, CancellationToken cancellationToken)
    {
        // Corregida por ForwardedHeadersMiddleware (ver Program.cs) — nunca la IP interna de nginx.
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString()
            ?? throw new InvalidOperationException("Request has no RemoteIpAddress.");

        var result = await _login.LoginAsync(new LoginRequest(body.Email, body.Password, ip), cancellationToken);
        var accessToken = _tokens.IssueAccessToken(result.UserId, result.SessionId);

        return Ok(new LoginResponse(accessToken, result.Memberships));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = long.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var sessionId = Guid.Parse(User.FindFirstValue(SessionClaims.SessionId)!);

        await _logout.LogoutAsync(userId, sessionId, cancellationToken);

        return NoContent();
    }
}
