using Takwene.Api.Models;
using Takwene.Api.Models.Enums;
namespace Takwene.Api.Repositories.Interfaces;
public interface ITrackRepository
{
    Task<List<Track>> ListAsync(int? artistId, string? genre, TrackStatus? status, CancellationToken ct);
    Task<Track?> FindAsync(int id, bool forUpdate, CancellationToken ct);
    Task<bool> IsrcExistsAsync(string isrc, CancellationToken ct);
    Task<int> CountDspsAsync(int[] ids, CancellationToken ct);
    Task AddAsync(Track track, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
    Task<T> WithTrackLockAsync<T>(int id, Func<Task<T>> action, CancellationToken ct);
}
