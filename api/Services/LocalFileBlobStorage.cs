using System.Security.Cryptography;

namespace SecureDocumentPortal.Api.Services;

public class LocalFileBlobStorage : IBlobStorage
{
    private readonly string _root;

    public LocalFileBlobStorage(IConfiguration cfg)
    {
        _root = cfg["LocalBlob:Root"] ?? Path.Combine(AppContext.BaseDirectory, "blob-store");
        Directory.CreateDirectory(_root);
    }

    public async Task<BlobUploadResult> UploadAsync(string container, string fileName, Stream content, string contentType, CancellationToken ct)
    {
        var dir = Path.Combine(_root, container, Path.GetDirectoryName(fileName) ?? "");
        Directory.CreateDirectory(dir);
        var fullPath = Path.Combine(_root, container, fileName);

        using var ms = new MemoryStream();
        await content.CopyToAsync(ms, ct);
        var bytes = ms.ToArray();
        var hash = Convert.ToHexString(SHA256.HashData(bytes));
        await File.WriteAllBytesAsync(fullPath, bytes, ct);

        var uri = $"file://{fullPath}";
        return new BlobUploadResult(uri, hash, bytes.Length);
    }

    public Task<Stream> DownloadAsync(string uri, CancellationToken ct)
    {
        var path = new Uri(uri).LocalPath;
        Stream s = File.OpenRead(path);
        return Task.FromResult(s);
    }

    public Task DeleteAsync(string uri, CancellationToken ct)
    {
        var path = new Uri(uri).LocalPath;
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }
}
