namespace Takwene.Api.Services;

// Service-only result: return Token through the HttpOnly cookie; persist only TokenHash.
public sealed record RefreshTokenResult(string Token, string TokenHash);
