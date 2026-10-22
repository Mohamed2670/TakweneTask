using AutoMapper;
using Takwene.Api.DTOs.Artists;
using Takwene.Api.Models;

namespace Takwene.Api.Profiles;

public sealed class ArtistProfile : Profile
{
    public ArtistProfile()
    {
        CreateMap<CreateArtistRequest, Artist>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.Tracks, options => options.Ignore());

        CreateMap<Artist, ArtistResponse>();
    }
}
