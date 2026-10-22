using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Takwene.Api.DTOs.Auth;
using Takwene.Api.Services.Interfaces;
using Takwene.Api.Validation;
namespace Takwene.Api.Controllers;
[ApiController]
[Route("api/auth")]
[ResponseCache(NoStore=true, Location=ResponseCacheLocation.None)]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    private const string CookieName = "__Secure-Takwene.Refresh";
    private const string CookiePath = "/api/auth";
    [HttpPost("login"), ServiceFilter(typeof(AuthRequestFilter))]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var session = await auth.LoginAsync(request, ct);
        if (session is null) return Problem(statusCode:401, title:"Invalid email or password.");
        SetRefreshCookie(session);
        return Ok(session.Response);
    }
    [HttpPost("refresh"), ServiceFilter(typeof(AuthRequestFilter))]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken ct)
    {
        var raw = Request.Cookies[CookieName];
        var session = raw is null ? null : await auth.RefreshAsync(raw, ct);
        if (session is null)
        {
            ClearRefreshCookie();
            return Problem(statusCode:401, title:"Refresh session is invalid or expired.");
        }
        SetRefreshCookie(session);
        return Ok(session.Response);
    }
    [HttpPost("logout"), ServiceFilter(typeof(AuthRequestFilter))]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var raw = Request.Cookies[CookieName];
        if (raw is not null) await auth.LogoutAsync(raw, ct);
        ClearRefreshCookie();
        return NoContent();
    }
    [Authorize, HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id)) return Unauthorized();
        var user = await auth.GetUserAsync(id, ct);
        return user is null ? Unauthorized() : Ok(user);
    }
    private static CookieOptions RefreshCookieOptions(DateTime? expires = null) => new()
    { HttpOnly=true, Secure=true, SameSite=SameSiteMode.None, Path=CookiePath, Expires=expires is null ? null : new DateTimeOffset(expires.Value, TimeSpan.Zero), IsEssential=true };
    private void SetRefreshCookie(AuthSession session) => Response.Cookies.Append(CookieName, session.RefreshToken, RefreshCookieOptions(session.RefreshExpiresAt));
    private void ClearRefreshCookie() => Response.Cookies.Delete(CookieName, RefreshCookieOptions());
}
