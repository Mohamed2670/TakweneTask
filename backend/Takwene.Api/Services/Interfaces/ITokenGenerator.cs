using Takwene.Api.Models;

namespace Takwene.Api.Services.Interfaces;

public interface ITokenGenerator
{
    AccessTokenResult GenerateAccessToken(User user);
}
