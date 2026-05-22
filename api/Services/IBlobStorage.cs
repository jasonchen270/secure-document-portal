namespace SecureDocumentPortal.Api.Services;

public record BlobUploadResult(string Uri, string Sha256, long SizeBytes);

public interface IBlobStorage
{
    Task<BlobUploadResult> UploadAsync(string container, string fileName, Stream content, string contentType, CancellationToken ct);
    Task<Stream> DownloadAsync(string uri, CancellationToken ct);
    Task DeleteAsync(string uri, CancellationToken ct);
}
