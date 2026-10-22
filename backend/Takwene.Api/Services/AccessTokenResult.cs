namespace Takwene.Api.Services;

// Internal service result; not a request or response DTO.
public sealed record AccessTokenResult(string Token, DateTime ExpiresAt);
