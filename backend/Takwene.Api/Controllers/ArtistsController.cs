using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Takwene.Api.DTOs.Artists;
using Takwene.Api.Services.Interfaces;
namespace Takwene.Api.Controllers;
[ApiController]
[Route("api/artists")]
public sealed class ArtistsController(IArtistService artists) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ArtistResponse>>> List(CancellationToken ct) => Ok(await artists.ListAsync(ct));
    [Authorize, HttpPost]
    public async Task<ActionResult<ArtistResponse>> Create(CreateArtistRequest request, CancellationToken ct) => CreatedAtAction(nameof(List),null,await artists.CreateAsync(request,ct));
}
