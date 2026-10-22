using Takwene.Api.DTOs.Tracks;
namespace Takwene.Api.Services.Interfaces;
public interface ITrackService
{
    Task<IReadOnlyList<TrackListResponse>> ListAsync(TrackQuery query, CancellationToken ct);
    Task<TrackDetailResponse> GetAsync(int id, CancellationToken ct);
    Task<TrackDetailResponse> CreateAsync(CreateTrackRequest request, CancellationToken ct);
    Task<TrackDetailResponse> DistributeAsync(int id, DistributeTrackRequest request, CancellationToken ct);
    Task<TrackDetailResponse> UpdateStatusAsync(int id, UpdateTrackStatusRequest request, CancellationToken ct);
}
