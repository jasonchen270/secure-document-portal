using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureDocumentPortal.Api.Auth;
using SecureDocumentPortal.Api.Data;

namespace SecureDocumentPortal.Api.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize(Policy = Policies.Reviewer)]
public class AuditController : ControllerBase
{
    private readonly AppDbContext _db;
    public AuditController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int take = 100)
    {
        take = Math.Clamp(take, 1, 500);
        var events = await _db.AuditEvents
            .OrderByDescending(e => e.OccurredAt)
            .Take(take)
            .ToListAsync();
        return Ok(events);
    }
}
