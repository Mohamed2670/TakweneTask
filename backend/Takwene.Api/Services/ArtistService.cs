using Takwene.Api.DTOs.Artists;
using Takwene.Api.Models;
using Takwene.Api.Repositories.Interfaces;
using Takwene.Api.Services.Interfaces;
using Takwene.Api.Validation;
namespace Takwene.Api.Services;
public sealed class ArtistService(IArtistRepository repository) : IArtistService
{
    public async Task<IReadOnlyList<ArtistResponse>> ListAsync(CancellationToken ct) => (await repository.ListAsync(ct)).Select(ToResponse).ToArray();
    public async Task<ArtistResponse> CreateAsync(CreateArtistRequest request, CancellationToken ct)
    {
        ModelValidator.Validate(request);
        var artist=new Artist { Name=request.Name, Email=request.Email, Country=request.Country };
        ModelValidator.Validate(artist);
        await repository.AddAsync(artist, ct);
        return ToResponse(artist);
    }
    private static ArtistResponse ToResponse(Artist artist) => new(artist.Id,artist.Name,artist.Email,artist.Country);
}
