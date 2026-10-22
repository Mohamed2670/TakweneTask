using Microsoft.EntityFrameworkCore;
using Takwene.Api.Data;
using Takwene.Api.Models;
using Takwene.Api.Models.Enums;
using Takwene.Api.Repositories.Interfaces;
namespace Takwene.Api.Repositories;
public sealed class TrackRepository(AppDbContext db) : ITrackRepository
{
    public Task<List<Track>> ListAsync(int? artistId, string? genre, TrackStatus? status, CancellationToken ct)
    {
        var query = db.Tracks.AsNoTracking().Include(x => x.Artist).AsQueryable();
        if (artistId.HasValue) query=query.Where(x => x.ArtistId == artistId.Value);
        if (genre is not null) query=query.Where(x => x.Genre == genre);
        if (status.HasValue) query=query.Where(x => x.Status == status.Value);
        return query.OrderBy(x => x.Id).ToListAsync(ct);
    }
    public Task<Track?> FindAsync(int id, bool forUpdate, CancellationToken ct)
    {
        var query = db.Tracks.Include(x => x.Artist).Include(x => x.Distributions).ThenInclude(x => x.Dsp).AsQueryable();
        if (!forUpdate) query=query.AsNoTracking();
        return query.SingleOrDefaultAsync(x => x.Id == id, ct);
    }
    public Task<bool> IsrcExistsAsync(string isrc, CancellationToken ct) => db.Tracks.AnyAsync(x => x.Isrc == isrc, ct);
    public Task<int> CountDspsAsync(int[] ids, CancellationToken ct) => db.Dsps.CountAsync(x => ids.Contains(x.Id), ct);
    public async Task AddAsync(Track track, CancellationToken ct) { db.Tracks.Add(track); await db.SaveChangesAsync(ct); }
    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
    public async Task<T> WithTrackLockAsync<T>(int id, Func<Task<T>> action, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var resource="track:" + id;
        await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @result int; EXEC @result=sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=5000; IF @result<0 THROW 51001, 'Track lock unavailable', 1;", ct);
        var result=await action();
        await transaction.CommitAsync(ct);
        return result;
    }
}
