using Takwene.Api.Models.Enums;
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
    [EnumDataType(typeof(TrackStatus))] public TrackStatus Status { get; set; } = TrackStatus.Draft;
    public ICollection<TrackDistribution> Distributions { get; set; } = new List<TrackDistribution>();
}

