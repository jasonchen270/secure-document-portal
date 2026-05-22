using System.Security.Cryptography;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace SecureDocumentPortal.Api.Services;

public class AzureBlobStorage : IBlobStorage
{
    private readonly BlobServiceClient _client;

    public AzureBlobStorage(IConfiguration cfg)
    {
        var conn = cfg.GetConnectionString("AzureBlob")
            ?? throw new InvalidOperationException("AzureBlob connection string not set");
        _client = new BlobServiceClient(conn);
    }

    public async Task<BlobUploadResult> UploadAsync(string container, string fileName, Stream content, string contentType, CancellationToken ct)
    {
        var c = _client.GetBlobContainerClient(container);
        await c.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

        using var ms = new MemoryStream();
        await content.CopyToAsync(ms, ct);
        ms.Position = 0;
        var hash = Convert.ToHexString(SHA256.HashData(ms.ToArray()));
        ms.Position = 0;

        var blob = c.GetBlobClient(fileName);
        await blob.UploadAsync(ms, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, ct);

        return new BlobUploadResult(blob.Uri.ToString(), hash, ms.Length);
    }

    public async Task<Stream> DownloadAsync(string uri, CancellationToken ct)
    {
        var blob = new BlobClient(new Uri(uri));
        var resp = await blob.DownloadStreamingAsync(cancellationToken: ct);
        return resp.Value.Content;
    }

    public async Task DeleteAsync(string uri, CancellationToken ct)
    {
        var blob = new BlobClient(new Uri(uri));
        await blob.DeleteIfExistsAsync(cancellationToken: ct);
    }
}
