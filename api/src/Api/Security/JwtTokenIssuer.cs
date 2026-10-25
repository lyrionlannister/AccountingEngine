using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Api.Security;

// Único lugar del proyecto que construye un JWT — Auth.Services nunca referencia esta
// librería (sigue siendo un detalle de transporte de Api, ver tasks/plan-auth.md).
public sealed class JwtTokenIssuer
{
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
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
