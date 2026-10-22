using System.ComponentModel.DataAnnotations;
namespace Takwene.Api.DTOs.Auth;
public sealed class LoginRequest
{
    private string email = "";
    [Required, EmailAddress, MaxLength(254)]
    public string Email { get => email; set => email = value?.Trim() ?? ""; }
    [Required, MinLength(1), MaxLength(256)]
    public string Password { get; set; } = "";
}
