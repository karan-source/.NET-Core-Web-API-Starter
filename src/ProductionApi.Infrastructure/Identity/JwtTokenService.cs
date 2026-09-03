using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using ProductionApi.Application.Common.Interfaces;

namespace ProductionApi.Infrastructure.Identity;

internal sealed class JwtTokenService(JwtOptions options, IDateTimeProvider clock) : IJwtTokenService
{
    public (string AccessToken, DateTimeOffset ExpiresAtUtc) CreateToken(
        string userId,
        string email,
        IEnumerable<string> roles)
    {
        var expiresAtUtc = clock.UtcNow.AddMinutes(options.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = options.Issuer,
            Audience = options.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAtUtc.UtcDateTime,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        };

        return (new JsonWebTokenHandler().CreateToken(descriptor), expiresAtUtc);
    }
}
