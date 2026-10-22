using AutoMapper;
using Takwene.Api.DTOs.Tracks;
using Takwene.Api.Models;

namespace Takwene.Api.Profiles;

public sealed class TrackProfile : Profile
{
    public TrackProfile()
    {
        CreateMap<CreateTrackRequest, Track>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.Status, options => options.Ignore())
            .ForMember(destination => destination.Artist, options => options.Ignore())
            .ForMember(destination => destination.Distributions, options => options.Ignore())
            .ForMember(destination => destination.ReleaseDate, options => options.MapFrom(source => source.ReleaseDate!.Value));

        CreateMap<Track, TrackListResponse>()
            .ForCtorParam(nameof(TrackListResponse.ArtistName), options => options.MapFrom(source => source.Artist.Name));

        CreateMap<Track, TrackDetailResponse>()
            .ForCtorParam(nameof(TrackDetailResponse.ArtistName), options => options.MapFrom(source => source.Artist.Name))
            .ForCtorParam(nameof(TrackDetailResponse.Distributions), options => options.MapFrom(source => source.Distributions.OrderBy(distribution => distribution.DspId)));

        CreateMap<TrackDistribution, DistributionResponse>()
            .ForCtorParam(nameof(DistributionResponse.DspName), options => options.MapFrom(source => source.Dsp.Name))
            .ForCtorParam(nameof(DistributionResponse.SubmittedAt), options => options.MapFrom(source => DateTime.SpecifyKind(source.SubmittedAt, DateTimeKind.Utc)));
    }
}
