using System.ComponentModel.DataAnnotations;

namespace Takwene.Api.Models;

public class Artist
{
    public int Id { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = "";
    [Required, EmailAddress, MaxLength(254)] public string Email { get; set; } = "";
    [Required, MaxLength(100)] public string Country { get; set; } = "";
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}
