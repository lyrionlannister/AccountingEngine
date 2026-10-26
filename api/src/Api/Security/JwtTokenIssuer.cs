using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Api.Security;

public sealed class JwtTokenIssuer
{
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
