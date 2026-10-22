using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Takwene.Api.DTOs.Tracks;
using Takwene.Api.Services.Interfaces;
namespace Takwene.Api.Controllers;
[ApiController]
[Route("api/tracks")]
public sealed class TracksController(ITrackService tracks) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TrackListResponse>>> List([FromQuery] TrackQuery query,CancellationToken ct) => Ok(await tracks.ListAsync(query,ct));
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TrackDetailResponse>> Get(int id,CancellationToken ct) => Ok(await tracks.GetAsync(id,ct));
    [Authorize, HttpPost]
    public async Task<ActionResult<TrackDetailResponse>> Create(CreateTrackRequest request,CancellationToken ct)
    {
        var track=await tracks.CreateAsync(request,ct);
        return CreatedAtAction(nameof(Get),new { id=track.Id },track);
    }
    [Authorize, HttpPost("{id:int}/distribute")]
    public async Task<ActionResult<TrackDetailResponse>> Distribute(int id,DistributeTrackRequest request,CancellationToken ct) => Ok(await tracks.DistributeAsync(id,request,ct));
    [Authorize, HttpPatch("{id:int}/status")]
    public async Task<ActionResult<TrackDetailResponse>> UpdateStatus(int id,UpdateTrackStatusRequest request,CancellationToken ct) => Ok(await tracks.UpdateStatusAsync(id,request,ct));
}
