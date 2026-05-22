using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureDocumentPortal.Api.Auth;
using SecureDocumentPortal.Api.Services;

namespace SecureDocumentPortal.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = Policies.Admin)]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;
    public UsersController(IUserService users) => _users = users;

    public record CreateUserRequest(string Email, string Password, string Role);

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var users = await _users.ListAsync();
        return Ok(users.Select(u => new { u.Id, u.Email, u.Role, u.IsActive, u.CreatedAt, u.LastLoginAt }));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest req)
    {
        if (req.Role != Roles.Admin && req.Role != Roles.Reviewer && req.Role != Roles.Uploader)
            return BadRequest(new { error = "invalid_role" });
        var existing = await _users.FindByEmailAsync(req.Email);
        if (existing is not null) return Conflict(new { error = "email_taken" });
        var u = await _users.CreateAsync(req.Email, req.Password, req.Role);
        return Created($"/api/users/{u.Id}", new { u.Id, u.Email, u.Role });
    }
}
