using Microsoft.EntityFrameworkCore;
using Takwene.Api.Data;
using Takwene.Api.Models;
using Takwene.Api.Repositories.Interfaces;
namespace Takwene.Api.Repositories;
public sealed class AuthRepository(AppDbContext db) : IAuthRepository
{
    public Task<User?> FindUserAsync(string email, CancellationToken ct) => db.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == email, ct);
    public Task<User?> FindUserByIdAsync(int id, CancellationToken ct) => db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
    public Task<RefreshToken?> FindRefreshTokenAsync(string hash, CancellationToken ct) => db.RefreshTokens.AsNoTracking().Include(x => x.User).SingleOrDefaultAsync(x => x.TokenHash == hash, ct);
    public void AddRefreshToken(RefreshToken token) => db.RefreshTokens.Add(token);
    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
    public Task LinkReplacementAsync(int oldId, int newId, CancellationToken ct) => db.RefreshTokens.Where(x => x.Id == oldId).ExecuteUpdateAsync(s => s.SetProperty(x => x.ReplacedByTokenId, newId), ct);
    public Task RevokeFamilyAsync(Guid familyId, DateTime now, CancellationToken ct) => db.RefreshTokens.Where(x => x.FamilyId == familyId && x.RevokedAt == null).ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, now), ct);
    public async Task<T> WithFamilyLockAsync<T>(Guid familyId, Func<Task<T>> action, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        // Serialize refresh and logout across application instances for the same session family.
        var resource = "refresh:" + familyId.ToString("N");
        await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @result int; EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=5000; IF @result < 0 THROW 51001, 'Session lock unavailable', 1;", ct);
        var result = await action();
        await transaction.CommitAsync(ct);
        return result;
    }
}

