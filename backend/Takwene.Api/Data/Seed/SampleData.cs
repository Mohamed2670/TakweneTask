using Takwene.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Takwene.Api.Models;
namespace Takwene.Api.Data.Seed;
public static class SampleData
{
    public static void Configure(ModelBuilder b)
    {
        b.Entity<Artist>().HasData(
            new Artist { Id=1, Name="Layla Noor", Email="layla@example.com", Country="Egypt" },
            new Artist { Id=2, Name="Omar Atlas", Email="omar@example.com", Country="Morocco" },
            new Artist { Id=3, Name="Nour Al Bahr", Email="nour@example.com", Country="Lebanon" });
        b.Entity<Dsp>().HasData(new Dsp { Id=1, Name="Spotify" }, new Dsp { Id=2, Name="Apple Music" }, new Dsp { Id=3, Name="YouTube" });
        string[] titles = ["Cairo Nights", "First Light", "Nile Breeze", "Atlas Road", "Casablanca Rain", "Desert Echo", "Beirut Morning", "Sea Letters", "Home Again"];
        string[] genres = ["Arabic Pop", "R&B", "Electronic", "Hip Hop", "Jazz", "Electronic", "Indie", "Arabic Pop", "Acoustic"];
        TrackStatus[] statuses = [TrackStatus.Draft, TrackStatus.Submitted, TrackStatus.Distributed];
        for (int i=0; i<titles.Length; i++)
            b.Entity<Track>().HasData(new Track { Id=i+1, ArtistId=i/3+1, Title=titles[i], Isrc=$"EGTKW26{i+1:D5}", ReleaseDate=new DateOnly(2026, 9, i+1), Genre=genres[i], Status=statuses[i%3] });
        b.Entity<TrackDistribution>().HasData(
            Distribution(1,2,1,DistributionStatus.Pending), Distribution(2,3,1,DistributionStatus.Live), Distribution(3,3,2,DistributionStatus.Live),
            Distribution(4,5,2,DistributionStatus.Rejected), Distribution(5,6,3,DistributionStatus.Live), Distribution(6,8,1,DistributionStatus.Pending),
            Distribution(7,9,2,DistributionStatus.Live), Distribution(8,9,3,DistributionStatus.Pending));
    }
    private static TrackDistribution Distribution(int id, int track, int dsp, DistributionStatus status) => new()
    { Id=id, TrackId=track, DspId=dsp, Status=status, SubmittedAt=new DateTime(2026,8,20,12,0,0,DateTimeKind.Utc) };
}

