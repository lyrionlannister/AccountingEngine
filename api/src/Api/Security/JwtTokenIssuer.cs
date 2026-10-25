using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Api.Security;

// Único lugar del proyecto que construye un JWT — Auth.Services nunca referencia esta
// librería (sigue siendo un detalle de transporte de Api, ver tasks/plan-auth.md).
public sealed class JwtTokenIssuer
{
    // Mismo valor fijo usado en la validación (Program.cs) — un monolito con un solo emisor y
    // un solo consumidor no necesita que sean configurables, pero validarlos explícitamente
    // (en vez de ValidateIssuer/ValidateAudience = false) cierra la puerta a que un token
    // firmado con la misma clave pero para otro propósito se acepte aquí si la clave se
    // reutiliza alguna vez.
    public const string Issuer = "AccountingEngine.Api";
    public const string Audience = "AccountingEngine.Api";

    private readonly SymmetricSecurityKey _signingKey;

    public JwtTokenIssuer(IConfiguration configuration)
    {
        var key = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Missing configuration 'Jwt:SigningKey'.");

        _signingKey = new SymmetricSecurityKey(Convert.FromBase64String(key));
    }

    public string IssueAccessToken(long userId, Guid sessionId)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(SessionClaims.SessionId, sessionId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
