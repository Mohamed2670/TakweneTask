using System.ComponentModel.DataAnnotations;
namespace Takwene.Api.Models;
public class Dsp
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = "";
    public ICollection<TrackDistribution> Distributions { get; set; } = new List<TrackDistribution>();
}
