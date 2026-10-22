using System.ComponentModel.DataAnnotations;
namespace Takwene.Api.Models;
public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    [Required, StringLength(64, MinimumLength = 64)] public string TokenHash { get; set; } = "";
    public Guid FamilyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public int? ReplacedByTokenId { get; set; }
    public RefreshToken? ReplacedByToken { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; } = [];
}
