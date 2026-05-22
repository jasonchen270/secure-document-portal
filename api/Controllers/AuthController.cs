using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureDocumentPortal.Api.Auth;
using SecureDocumentPortal.Api.Data;
using SecureDocumentPortal.Api.Domain;
using SecureDocumentPortal.Api.Services;

namespace SecureDocumentPortal.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _users;
    private readonly ITokenService _tokens;
    private readonly IAuditLogger _audit;
    private readonly AppDbContext _db;

    public AuthController(IUserService users, ITokenService tokens, IAuditLogger audit, AppDbContext db)
    {
        _users = users;
        _tokens = tokens;
        _audit = audit;
        _db = db;
    }

    public record LoginRequest(string Email, string Password);
    public record RefreshRequest(string RefreshToken);
    public record LoginResponse(string AccessToken, string RefreshToken, DateTime AccessExpiresAt, string Role, string Email);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var user = await _users.FindByEmailAsync(req.Email);
        if (user is null || !user.IsActive || !await _users.VerifyPasswordAsync(user, req.Password))
        {
            await _audit.LogAsync("auth.login.fail", user?.Id, "User", req.Email, ip);
            return Unauthorized(new { error = "invalid_credentials" });
        }

        var auth = _tokens.Issue(user);
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokens.HashRefreshToken(auth.RefreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(14)
        });
        await _users.UpdateLastLoginAsync(user);
        await _audit.LogAsync("auth.login.success", user.Id, "User", user.Id.ToString(), ip);
        return Ok(new LoginResponse(auth.AccessToken, auth.RefreshToken, auth.AccessExpiresAt, user.Role, user.Email));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
    {
        var hash = _tokens.HashRefreshToken(req.RefreshToken);
        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(r =>
            r.TokenHash == hash && r.RevokedAt == null && r.ExpiresAt > DateTime.UtcNow);
        if (stored is null) return Unauthorized();

        var user = await _users.FindByIdAsync(stored.UserId);
        if (user is null || !user.IsActive) return Unauthorized();

        var auth = _tokens.Issue(user);
        var newHash = _tokens.HashRefreshToken(auth.RefreshToken);
        stored.RevokedAt = DateTime.UtcNow;
        stored.ReplacedByHash = newHash;
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newHash,
            ExpiresAt = DateTime.UtcNow.AddDays(14)
        });
        await _db.SaveChangesAsync();
        return Ok(new LoginResponse(auth.AccessToken, auth.RefreshToken, auth.AccessExpiresAt, user.Role, user.Email));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req)
    {
        var hash = _tokens.HashRefreshToken(req.RefreshToken);
        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash);
        if (stored is not null && stored.RevokedAt is null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        return NoContent();
    }
}
