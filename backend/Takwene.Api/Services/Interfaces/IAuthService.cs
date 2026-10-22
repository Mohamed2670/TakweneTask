using Takwene.Api.DTOs.Auth;
namespace Takwene.Api.Services.Interfaces;
public sealed record AuthSession(AuthResponse Response, string RefreshToken, DateTime RefreshExpiresAt);
public interface IAuthService
{
    Task<AuthSession?> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<AuthSession?> RefreshAsync(string token, CancellationToken ct);
    Task LogoutAsync(string token, CancellationToken ct);
    Task<UserResponse?> GetUserAsync(int id, CancellationToken ct);
}
