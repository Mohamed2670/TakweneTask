using Takwene.Api.Models;
namespace Takwene.Api.Repositories.Interfaces;
public interface IArtistRepository
{
    Task<List<Artist>> ListAsync(CancellationToken ct);
    Task<bool> ExistsAsync(int id, CancellationToken ct);
    Task AddAsync(Artist artist, CancellationToken ct);
}
