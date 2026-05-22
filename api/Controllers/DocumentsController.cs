using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureDocumentPortal.Api.Auth;
using SecureDocumentPortal.Api.Services;

namespace SecureDocumentPortal.Api.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _docs;
    public DocumentsController(IDocumentService docs) => _docs = docs;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!);
    private string Role => User.FindFirstValue(ClaimTypes.Role) ?? "";

    public record CreateDocumentRequest(string Title, string Classification);

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var docs = await _docs.ListVisibleToAsync(UserId, Role);
        return Ok(docs.Select(d => new
        {
            d.Id,
            d.Title,
            d.Classification,
            d.OwnerId,
            d.UpdatedAt,
            LatestVersion = d.Versions.OrderByDescending(v => v.Version).FirstOrDefault()?.Version,
            VersionCount = d.Versions.Count
        }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var doc = await _docs.GetAsync(id, UserId, Role);
        return doc is null ? NotFound() : Ok(doc);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Uploader)]
    public async Task<IActionResult> Create([FromBody] CreateDocumentRequest req)
    {
        var doc = await _docs.CreateAsync(req.Title, req.Classification, UserId);
        return CreatedAtAction(nameof(Get), new { id = doc.Id }, doc);
    }

    [HttpPost("{id:guid}/versions")]
    [Authorize(Policy = Policies.Uploader)]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> AddVersion(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0) return BadRequest(new { error = "file_required" });
        var version = await _docs.AddVersionAsync(id, file, UserId, ct);
        return Ok(new { version.Id, version.Version, version.Sha256, version.SizeBytes });
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var result = await _docs.DownloadLatestAsync(id, UserId, Role, ct);
        if (result is null) return NotFound();
        return File(result.Value.content, result.Value.contentType, result.Value.fileName);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _docs.SoftDeleteAsync(id, UserId, Role);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
