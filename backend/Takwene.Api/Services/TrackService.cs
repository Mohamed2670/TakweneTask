using Takwene.Api.DTOs.Tracks;
using Takwene.Api.Models;
using Takwene.Api.Models.Enums;
using Takwene.Api.Repositories.Interfaces;
using Takwene.Api.Services.Interfaces;
using Takwene.Api.Validation;
namespace Takwene.Api.Services;
public sealed class TrackService(ITrackRepository tracks, IArtistRepository artists) : ITrackService
{
    public async Task<IReadOnlyList<TrackListResponse>> ListAsync(TrackQuery query, CancellationToken ct)
    {
        ModelValidator.Validate(query);
        TrackStatus? status=query.Status is null ? null : Enum.Parse<TrackStatus>(query.Status, true);
        return (await tracks.ListAsync(query.ArtistId, query.Genre, status, ct))
            .Select(x => new TrackListResponse(x.Id,x.Title,x.ArtistId,x.Artist.Name,x.ReleaseDate,x.Genre,x.Status)).ToArray();
    }
    public async Task<TrackDetailResponse> GetAsync(int id, CancellationToken ct)
    {
        ValidateId(id);
        return ToDetail(await tracks.FindAsync(id, false, ct) ?? throw new ApiException(404,"Track not found."));
    }
    public async Task<TrackDetailResponse> CreateAsync(CreateTrackRequest request, CancellationToken ct)
    {
        ModelValidator.Validate(request);
        if (!await artists.ExistsAsync(request.ArtistId, ct)) throw new ApiException(400,"The referenced artist does not exist.",new Dictionary<string,string[]> { ["artistId"]=["Choose an existing artist."] });
        if (await tracks.IsrcExistsAsync(request.Isrc, ct)) throw new ApiException(409,"A track with this ISRC already exists.");
        var track=new Track { Title=request.Title, ArtistId=request.ArtistId, Isrc=request.Isrc, ReleaseDate=request.ReleaseDate!.Value, Genre=request.Genre, Status=TrackStatus.Draft };
        ModelValidator.Validate(track);
        await tracks.AddAsync(track,ct);
        return await GetAsync(track.Id,ct);
    }
    public Task<TrackDetailResponse> DistributeAsync(int id, DistributeTrackRequest request, CancellationToken ct)
    {
        ValidateId(id);
        ModelValidator.Validate(request);
        return tracks.WithTrackLockAsync(id,async () =>
        {
            var track=await tracks.FindAsync(id,true,ct) ?? throw new ApiException(404,"Track not found.");
            if (track.Status == TrackStatus.Distributed) throw new ApiException(409,"A distributed track cannot be submitted again.");
            var ids=request.DspIds!;
            if (await tracks.CountDspsAsync(ids,ct) != ids.Length) throw new ApiException(400,"One or more DSPs do not exist.",new Dictionary<string,string[]> { ["dspIds"]=["Choose existing DSP IDs."] });
            if (track.Distributions.Any(x => ids.Contains(x.DspId))) throw new ApiException(409,"The track has already been submitted to one or more requested DSPs.");
            var now=DateTime.UtcNow;
            foreach (var dspId in ids)
            {
                var distribution=new TrackDistribution { TrackId=id,DspId=dspId,SubmittedAt=now,Status=DistributionStatus.Pending };
                ModelValidator.Validate(distribution);
                track.Distributions.Add(distribution);
            }
            if (track.Status == TrackStatus.Draft) track.Status=TrackStatus.Submitted;
            ModelValidator.Validate(track);
            await tracks.SaveAsync(ct);
            return await GetAsync(id,ct);
        },ct);
    }
    public Task<TrackDetailResponse> UpdateStatusAsync(int id, UpdateTrackStatusRequest request, CancellationToken ct)
    {
        ValidateId(id);
        ModelValidator.Validate(request);
        return tracks.WithTrackLockAsync(id,async () =>
        {
            var track=await tracks.FindAsync(id,true,ct) ?? throw new ApiException(404,"Track not found.");
            var target=request.Status!.Value;
            if (target != track.Status && (int)target != (int)track.Status+1)
                throw new ApiException(409,"Status must advance from draft to submitted to distributed; backward or skipped transitions are not allowed.");
            track.Status=target;
            ModelValidator.Validate(track);
            await tracks.SaveAsync(ct);
            return await GetAsync(id,ct);
        },ct);
    }
    private static void ValidateId(int id) { if (id<=0) throw new ApiException(400,"Track ID must be positive."); }
    private static TrackDetailResponse ToDetail(Track track) => new(track.Id,track.Title,track.ArtistId,track.Artist.Name,track.Isrc,track.ReleaseDate,track.Genre,track.Status,
        track.Distributions.OrderBy(x => x.DspId).Select(x => new DistributionResponse(x.Id,x.DspId,x.Dsp.Name,DateTime.SpecifyKind(x.SubmittedAt,DateTimeKind.Utc),x.Status)).ToArray());
}
