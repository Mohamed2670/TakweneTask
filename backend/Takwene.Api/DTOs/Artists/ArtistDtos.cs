using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Takwene.Api.DTOs.Artists;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CreateArtistRequest
{
    private string name = "", email = "", country = "";
    [Required, MaxLength(150)]
    public string Name { get => name; set => name = value?.Trim() ?? ""; }
    [Required, EmailAddress, MaxLength(254)]
    public string Email { get => email; set => email = value?.Trim() ?? ""; }
    [Required, MaxLength(100)]
    public string Country { get => country; set => country = value?.Trim() ?? ""; }
}
public sealed record ArtistResponse(int Id, string Name, string Email, string Country);
