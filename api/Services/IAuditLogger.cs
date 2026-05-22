namespace SecureDocumentPortal.Api.Services;

public interface IAuditLogger
{
    Task LogAsync(string action, Guid? actorId, string? targetType = null, string? targetId = null, string? ip = null, string? metadata = null);
}
