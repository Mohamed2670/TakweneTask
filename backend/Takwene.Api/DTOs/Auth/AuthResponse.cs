namespace Takwene.Api.DTOs.Auth;
public sealed record UserResponse(int Id, string Name, string Email);
public sealed record AuthResponse(string AccessToken, DateTime ExpiresAt, UserResponse User);
