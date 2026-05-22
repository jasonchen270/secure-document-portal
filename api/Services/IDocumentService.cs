using SecureDocumentPortal.Api.Domain;

namespace SecureDocumentPortal.Api.Services;

public interface IDocumentService
{
    Task<List<Document>> ListVisibleToAsync(Guid userId, string role);
    Task<Document?> GetAsync(Guid id, Guid userId, string role);
    Task<Document> CreateAsync(string title, string classification, Guid ownerId);
    Task<DocumentVersion> AddVersionAsync(Guid documentId, IFormFile file, Guid uploaderId, CancellationToken ct);
    Task<(Stream content, string contentType, string fileName)?> DownloadLatestAsync(Guid documentId, Guid userId, string role, CancellationToken ct);
    Task SoftDeleteAsync(Guid documentId, Guid userId, string role);
}
