using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Takwene.Api.Models.Enums;
namespace Takwene.Api.DTOs.Tracks;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CreateTrackRequest : IValidatableObject
{
    private string title = "", isrc = "", genre = "";
    [Required, MaxLength(200)]
    public string Title { get => title; set => title = value?.Trim() ?? ""; }
    [Range(1, int.MaxValue)] public int ArtistId { get; set; }
    [Required, RegularExpression("^[A-Z]{2}[A-Z0-9]{3}[0-9]{7}$", ErrorMessage="ISRC must contain 2 letters, 3 alphanumeric characters, then 7 digits.")]
    public string Isrc { get => isrc; set => isrc = (value?.Trim() ?? "").Replace("-", "").ToUpperInvariant(); }
    [Required] public DateOnly? ReleaseDate { get; set; }
    [Required, MaxLength(100)]
    public string Genre { get => genre; set => genre = value?.Trim() ?? ""; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (ReleaseDate == DateOnly.MinValue) yield return new ValidationResult("Release date must be a valid non-default date.", [nameof(ReleaseDate)]);
    }
}
public sealed class TrackQuery
{
    private string? genre, status;
    [Range(1, int.MaxValue)] public int? ArtistId { get; set; }
    [MaxLength(100)] public string? Genre { get => genre; set => genre = string.IsNullOrWhiteSpace(value) ? null : value.Trim(); }
    [RegularExpression("^(draft|submitted|distributed)$", ErrorMessage="Status must be draft, submitted, or distributed.")]
    public string? Status { get => status; set => status = string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant(); }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class DistributeTrackRequest : IValidatableObject
{
    [Required, MinLength(1), MaxLength(100)] public int[]? DspIds { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (DspIds is null) yield break;
        if (DspIds.Any(id => id <= 0)) yield return new ValidationResult("DSP IDs must be positive.", [nameof(DspIds)]);
        if (DspIds.Distinct().Count() != DspIds.Length) yield return new ValidationResult("DSP IDs must not contain duplicates.", [nameof(DspIds)]);
    }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class UpdateTrackStatusRequest
{
    [Required, EnumDataType(typeof(TrackStatus))] public TrackStatus? Status { get; set; }
}
public sealed record TrackListResponse(int Id, string Title, int ArtistId, string ArtistName, DateOnly ReleaseDate, string Genre, TrackStatus Status);
public sealed record DistributionResponse(int Id, int DspId, string DspName, DateTime SubmittedAt, DistributionStatus Status);
public sealed record TrackDetailResponse(int Id, string Title, int ArtistId, string ArtistName, string Isrc, DateOnly ReleaseDate, string Genre, TrackStatus Status, IReadOnlyList<DistributionResponse> Distributions);
