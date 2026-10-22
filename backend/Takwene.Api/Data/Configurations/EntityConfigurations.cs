using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Takwene.Api.Models;
namespace Takwene.Api.Data.Configurations;
public class ArtistConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> b) { b.ToTable("Artists"); b.HasMany(x => x.Tracks).WithOne(x => x.Artist).HasForeignKey(x => x.ArtistId).OnDelete(DeleteBehavior.Restrict); }
}
public class TrackConfiguration : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> b)
    {
        b.ToTable("Tracks", t => t.HasCheckConstraint("CK_Tracks_Status", "[Status] IN ('draft','submitted','distributed')"));
        b.Property(x => x.Isrc).HasColumnType("varchar(12)").IsRequired();
        b.Property(x => x.Status).HasColumnType("varchar(20)").IsRequired();
        b.HasIndex(x => x.Isrc).IsUnique();
        b.HasMany(x => x.Distributions).WithOne(x => x.Track).HasForeignKey(x => x.TrackId).OnDelete(DeleteBehavior.Restrict);
    }
}
public class DspConfiguration : IEntityTypeConfiguration<Dsp>
{
    public void Configure(EntityTypeBuilder<Dsp> b)
    {
        b.ToTable("DSPs"); b.HasIndex(x => x.Name).IsUnique();
        b.HasMany(x => x.Distributions).WithOne(x => x.Dsp).HasForeignKey(x => x.DspId).OnDelete(DeleteBehavior.Restrict);
    }
}
public class DistributionConfiguration : IEntityTypeConfiguration<TrackDistribution>
{
    public void Configure(EntityTypeBuilder<TrackDistribution> b)
    {
        b.ToTable("TrackDistributions", t => t.HasCheckConstraint("CK_Distributions_Status", "[Status] IN ('pending','live','rejected')"));
        b.Property(x => x.Status).HasColumnType("varchar(20)").IsRequired();
        b.HasIndex(x => new { x.TrackId, x.DspId }).IsUnique();
    }
}
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users"); b.HasIndex(x => x.NormalizedEmail).IsUnique();
        b.HasMany(x => x.RefreshTokens).WithOne(x => x.User).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("RefreshTokens", t => t.HasCheckConstraint("CK_RefreshTokens_Expiry", "[ExpiresAt] > [CreatedAt]"));
        b.Property(x => x.TokenHash).HasColumnType("char(64)").IsRequired();
        b.HasIndex(x => x.TokenHash).IsUnique(); b.HasIndex(x => x.FamilyId);
        b.HasOne(x => x.ReplacedByToken).WithMany().HasForeignKey(x => x.ReplacedByTokenId).OnDelete(DeleteBehavior.Restrict);
    }
}

