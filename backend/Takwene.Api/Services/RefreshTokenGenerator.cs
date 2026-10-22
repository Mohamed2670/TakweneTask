using System.Security.Cryptography;
using System.Text;
using Takwene.Api.Services.Interfaces;

namespace Takwene.Api.Services;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    public RefreshTokenResult GenerateRefreshToken()
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        return new RefreshTokenResult(token, HashToken(token));
    }

    public string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
