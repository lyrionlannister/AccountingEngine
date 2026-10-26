using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Api.RateLimiting;
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
    private readonly LoginAccountRateLimiter _accountRateLimiter;

    public AuthController(LoginService login, LogoutService logout, JwtTokenIssuer tokens, LoginAccountRateLimiter accountRateLimiter)
    {
        _login = login;
        _logout = logout;
        _tokens = tokens;
        _accountRateLimiter = accountRateLimiter;
    }

    public sealed record LoginBody(
        [Required(ErrorMessage = "El correo es obligatorio.")] string Email,
        [Required(ErrorMessage = "La contraseña es obligatoria.")] string Password);

    public sealed record LoginResponse(string AccessToken, IReadOnlyList<MembershipSummary> Memberships);

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginBody body, CancellationToken cancellationToken)
    {
        if (!_accountRateLimiter.TryAcquire(body.Email))
            return Problem(
                statusCode: StatusCodes.Status429TooManyRequests,
                title: "Demasiados intentos",
                detail: "Demasiados intentos de inicio de sesión para esta cuenta. Intenta de nuevo más tarde.");

        var remoteIp = HttpContext.Connection.RemoteIpAddress
            ?? throw new InvalidOperationException("Request has no RemoteIpAddress.");
        var ip = (remoteIp.IsIPv4MappedToIPv6 ? remoteIp.MapToIPv4() : remoteIp).ToString();

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
