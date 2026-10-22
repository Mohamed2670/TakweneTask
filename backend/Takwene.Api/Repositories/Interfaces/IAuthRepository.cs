using Takwene.Api.Models;
namespace Takwene.Api.Repositories.Interfaces;
public interface IAuthRepository
{
    Task<User?> FindUserAsync(string normalizedEmail, CancellationToken ct);
    Task<User?> FindUserByIdAsync(int id, CancellationToken ct);
    Task<RefreshToken?> FindRefreshTokenAsync(string hash, CancellationToken ct);
    void AddRefreshToken(RefreshToken token);
    Task SaveAsync(CancellationToken ct);
    Task LinkReplacementAsync(int oldId, int newId, CancellationToken ct);
    Task RevokeFamilyAsync(Guid familyId, DateTime now, CancellationToken ct);
    Task<T> WithFamilyLockAsync<T>(Guid familyId, Func<Task<T>> action, CancellationToken ct);
}

