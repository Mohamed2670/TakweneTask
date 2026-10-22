using Microsoft.EntityFrameworkCore;
using Takwene.Api.Data;
using Takwene.Api.Models;
using Takwene.Api.Repositories.Interfaces;
namespace Takwene.Api.Repositories;
public sealed class ArtistRepository(AppDbContext db) : IArtistRepository
{
    public Task<List<Artist>> ListAsync(CancellationToken ct) => db.Artists.AsNoTracking().OrderBy(x => x.Id).ToListAsync(ct);
    public Task<bool> ExistsAsync(int id, CancellationToken ct) => db.Artists.AnyAsync(x => x.Id == id, ct);
    public async Task AddAsync(Artist artist, CancellationToken ct)
    {
        db.Artists.Add(artist);
        await db.SaveChangesAsync(ct);
    }
}
