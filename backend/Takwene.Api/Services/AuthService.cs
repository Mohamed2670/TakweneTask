using AutoMapper;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Takwene.Api.DTOs.Auth;
using Takwene.Api.Models;
using Takwene.Api.Options;
using Takwene.Api.Repositories.Interfaces;
using Takwene.Api.Services.Interfaces;
namespace Takwene.Api.Services;
public sealed class AuthService(
    IAuthRepository repository,
    ITokenGenerator tokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IPasswordHasher<User> hasher,
    IMapper mapper) : IAuthService
{
    private static readonly string DummyHash = new PasswordHasher<User>().HashPassword(new User(), Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
    public async Task<AuthSession?> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await repository.FindUserAsync(request.Email.Trim().ToUpperInvariant(), ct);
        var verification = hasher.VerifyHashedPassword(user ?? new User(), user?.PasswordHash ?? DummyHash, request.Password);
        if (user is null || !user.IsActive || verification == PasswordVerificationResult.Failed) return null;
        if (verification == PasswordVerificationResult.SuccessRehashNeeded) user.PasswordHash = hasher.HashPassword(user, request.Password);
        var now = DateTime.UtcNow;
        var generated = refreshTokenGenerator.GenerateRefreshToken();
        var refresh = CreateRefresh(user.Id, generated, Guid.NewGuid(), now, now.AddDays(JwtOptions.RefreshTokenDays));
        repository.AddRefreshToken(refresh);
        await repository.SaveAsync(ct);
        return new AuthSession(CreateAuthResponse(user), generated.Token, refresh.ExpiresAt);
    }
    public async Task<AuthSession?> RefreshAsync(string raw, CancellationToken ct)
    {
        if (raw.Length != 64) return null;
        var hash = refreshTokenGenerator.HashToken(raw);
        var initial = await repository.FindRefreshTokenAsync(hash, ct);
        if (initial is null) return null;
        return await repository.WithFamilyLockAsync<AuthSession?>(initial.FamilyId, async () =>
        {
            var current = await repository.FindRefreshTokenAsync(hash, ct);
            var now = DateTime.UtcNow;
            if (current is null) return null;
            if (current.RevokedAt is not null || !current.User.IsActive)
            {
                await repository.RevokeFamilyAsync(current.FamilyId, now, ct);
                return null;
            }
            if (current.ExpiresAt <= now) return null;
            var nextGenerated = refreshTokenGenerator.GenerateRefreshToken();
            var next = CreateRefresh(current.UserId, nextGenerated, current.FamilyId, now, current.ExpiresAt);
            repository.AddRefreshToken(next);
            // Mark the old token through a conditional database update, with the family lock held.
            await repository.RevokeFamilyAsync(current.FamilyId, now, ct);
            await repository.SaveAsync(ct);
            await repository.LinkReplacementAsync(current.Id, next.Id, ct);
            return new AuthSession(CreateAuthResponse(current.User), nextGenerated.Token, next.ExpiresAt);
        }, ct);
    }
    public async Task LogoutAsync(string raw, CancellationToken ct)
    {
        if (raw.Length != 64) return;
        var token = await repository.FindRefreshTokenAsync(refreshTokenGenerator.HashToken(raw), ct);
        if (token is null) return;
        await repository.WithFamilyLockAsync(token.FamilyId, async () =>
        {
            await repository.RevokeFamilyAsync(token.FamilyId, DateTime.UtcNow, ct);
            return true;
        }, ct);
    }
    public async Task<UserResponse?> GetUserAsync(int id, CancellationToken ct)
    {
        var user = await repository.FindUserByIdAsync(id, ct);
        return user is null ? null : mapper.Map<UserResponse>(user);
    }
    private AuthResponse CreateAuthResponse(User user)
    {
        var access = tokenGenerator.GenerateAccessToken(user);
        return new AuthResponse(access.Token, access.ExpiresAt, mapper.Map<UserResponse>(user));
    }
    private static RefreshToken CreateRefresh(int userId, RefreshTokenResult generated, Guid family, DateTime now, DateTime expires) => new()
    { UserId=userId, TokenHash=generated.TokenHash, FamilyId=family, CreatedAt=now, ExpiresAt=expires };
}
