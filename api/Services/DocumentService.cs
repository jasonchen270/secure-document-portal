using Microsoft.EntityFrameworkCore;
using SecureDocumentPortal.Api.Auth;
using SecureDocumentPortal.Api.Data;
using SecureDocumentPortal.Api.Domain;

namespace SecureDocumentPortal.Api.Services;

public class DocumentService : IDocumentService
{
    private const string Container = "documents";

    private readonly AppDbContext _db;
    private readonly IBlobStorage _blobs;
    private readonly IAuditLogger _audit;

    public DocumentService(AppDbContext db, IBlobStorage blobs, IAuditLogger audit)
    {
        _db = db;
        _blobs = blobs;
        _audit = audit;
    }

    public Task<List<Document>> ListVisibleToAsync(Guid userId, string role)
    {
        var q = _db.Documents.Include(d => d.Versions).Where(d => !d.IsDeleted);
        if (role == Roles.Uploader)
            q = q.Where(d => d.OwnerId == userId);
        return q.OrderByDescending(d => d.UpdatedAt).ToListAsync();
    }

    public async Task<Document?> GetAsync(Guid id, Guid userId, string role)
    {
        var doc = await _db.Documents
            .Include(d => d.Versions)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        if (doc is null) return null;
        if (role == Roles.Uploader && doc.OwnerId != userId) return null;
        return doc;
    }

    public async Task<Document> CreateAsync(string title, string classification, Guid ownerId)
    {
        var doc = new Document
        {
            Title = title,
            Classification = classification,
            OwnerId = ownerId
        };
        _db.Documents.Add(doc);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("document.create", ownerId, "Document", doc.Id.ToString());
        return doc;
    }

    public async Task<DocumentVersion> AddVersionAsync(Guid documentId, IFormFile file, Guid uploaderId, CancellationToken ct)
    {
        var doc = await _db.Documents.Include(d => d.Versions)
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted, ct)
            ?? throw new KeyNotFoundException("Document not found");

        var nextVersion = (doc.Versions.Max(v => (int?)v.Version) ?? 0) + 1;
        var fileName = $"{documentId}/v{nextVersion}-{Guid.NewGuid():N}-{Path.GetFileName(file.FileName)}";

        await using var stream = file.OpenReadStream();
        var upload = await _blobs.UploadAsync(Container, fileName, stream, file.ContentType, ct);

        var version = new DocumentVersion
        {
            DocumentId = doc.Id,
            Version = nextVersion,
            BlobUri = upload.Uri,
            Sha256 = upload.Sha256,
            SizeBytes = upload.SizeBytes,
            ContentType = file.ContentType,
            UploadedById = uploaderId
        };
        _db.DocumentVersions.Add(version);
        doc.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync("document.version.add", uploaderId, "DocumentVersion", version.Id.ToString(),
            metadata: $"sha256={upload.Sha256};size={upload.SizeBytes}");
        return version;
    }

    public async Task<(Stream content, string contentType, string fileName)?> DownloadLatestAsync(Guid documentId, Guid userId, string role, CancellationToken ct)
    {
        var doc = await GetAsync(documentId, userId, role);
        if (doc is null) return null;
        var latest = doc.Versions.OrderByDescending(v => v.Version).FirstOrDefault();
        if (latest is null) return null;

        var stream = await _blobs.DownloadAsync(latest.BlobUri, ct);
        await _audit.LogAsync("document.download", userId, "DocumentVersion", latest.Id.ToString());
        return (stream, latest.ContentType, $"{doc.Title}-v{latest.Version}");
    }

    public async Task SoftDeleteAsync(Guid documentId, Guid userId, string role)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
        if (doc is null) return;
        if (role != Roles.Admin && doc.OwnerId != userId)
            throw new UnauthorizedAccessException();

        doc.IsDeleted = true;
        doc.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("document.delete", userId, "Document", doc.Id.ToString());
    }
}
