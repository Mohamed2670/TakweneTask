namespace Takwene.Api.Services.Interfaces;

public interface IRefreshTokenGenerator
{
    RefreshTokenResult GenerateRefreshToken();
    string HashToken(string token);
}
