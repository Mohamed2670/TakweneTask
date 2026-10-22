using System.ComponentModel.DataAnnotations;
namespace Takwene.Api.Models;
public class Track
{
    public int Id { get; set; }
    [Required, MaxLength(200)] public string Title { get; set; } = "";
    public int ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;
    [Required, RegularExpression("^[A-Z]{2}[A-Z0-9]{3}[0-9]{7}$")] public string Isrc { get; set; } = "";
    public DateOnly ReleaseDate { get; set; }
    [Required, MaxLength(100)] public string Genre { get; set; } = "";
    [Required, RegularExpression("^(draft|submitted|distributed)$")] public string Status { get; set; } = "draft";
    public ICollection<TrackDistribution> Distributions { get; set; } = new List<TrackDistribution>();
}

