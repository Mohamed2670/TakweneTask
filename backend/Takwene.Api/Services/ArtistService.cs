using AutoMapper;
using Takwene.Api.DTOs.Artists;
using Takwene.Api.Models;
using Takwene.Api.Repositories.Interfaces;
using Takwene.Api.Services.Interfaces;
using Takwene.Api.Validation;

namespace Takwene.Api.Services;

public sealed class ArtistService(IArtistRepository repository, IMapper mapper) : IArtistService
{
    public async Task<IReadOnlyList<ArtistResponse>> ListAsync(CancellationToken ct) =>
        mapper.Map<ArtistResponse[]>(await repository.ListAsync(ct));

    public async Task<ArtistResponse> CreateAsync(CreateArtistRequest request, CancellationToken ct)
    {
        ModelValidator.Validate(request);
        var artist = mapper.Map<Artist>(request);
        ModelValidator.Validate(artist);
        await repository.AddAsync(artist, ct);
        return mapper.Map<ArtistResponse>(artist);
    }
}
