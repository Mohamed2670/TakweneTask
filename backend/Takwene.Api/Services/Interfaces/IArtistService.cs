using Takwene.Api.DTOs.Artists;
namespace Takwene.Api.Services.Interfaces;
public interface IArtistService
{
    Task<IReadOnlyList<ArtistResponse>> ListAsync(CancellationToken ct);
    Task<ArtistResponse> CreateAsync(CreateArtistRequest request, CancellationToken ct);
}
