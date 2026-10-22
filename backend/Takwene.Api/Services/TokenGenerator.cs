using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Takwene.Api.Models;
using Takwene.Api.Options;
using Takwene.Api.Services.Interfaces;

namespace Takwene.Api.Services;

public sealed class TokenGenerator(IOptions<JwtOptions> options) : ITokenGenerator
{
    public AccessTokenResult GenerateAccessToken(User user)
    {
        var settings = options.Value;
        // JWT NumericDate values have whole-second precision. Use the same instant for every field.
        var now = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds()).UtcDateTime;
        var expiration = now.AddMinutes(JwtOptions.AccessTokenMinutes);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expiration,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)),
                SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            ])
        };

        var handler = new JwtSecurityTokenHandler();
        var securityToken = handler.CreateToken(descriptor);
        return new AccessTokenResult(handler.WriteToken(securityToken), securityToken.ValidTo);
    }
}
