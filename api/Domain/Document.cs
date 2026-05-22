namespace SecureDocumentPortal.Api.Domain;

public class Document
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = default!;
    public string Classification { get; set; } = "Internal";
    public Guid OwnerId { get; set; }
    public User? Owner { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
    public List<DocumentVersion> Versions { get; set; } = new();
}

public class DocumentVersion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DocumentId { get; set; }
    public Document? Document { get; set; }
    public int Version { get; set; }
    public string BlobUri { get; set; } = default!;
    public string Sha256 { get; set; } = default!;
    public long SizeBytes { get; set; }
    public string ContentType { get; set; } = "application/octet-stream";
    public Guid UploadedById { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
