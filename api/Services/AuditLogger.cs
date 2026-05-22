using SecureDocumentPortal.Api.Data;
using SecureDocumentPortal.Api.Domain;

namespace SecureDocumentPortal.Api.Services;

public class AuditLogger : IAuditLogger
{
    private readonly AppDbContext _db;
    private readonly ILogger<AuditLogger> _log;

    public AuditLogger(AppDbContext db, ILogger<AuditLogger> log)
    {
        _db = db;
        _log = log;
    }

    public async Task LogAsync(string action, Guid? actorId, string? targetType = null, string? targetId = null, string? ip = null, string? metadata = null)
    {
        var evt = new AuditEvent
        {
            Action = action,
            ActorId = actorId,
            TargetType = targetType,
            TargetId = targetId,
            IpAddress = ip,
            Metadata = metadata
        };
        _db.AuditEvents.Add(evt);
        await _db.SaveChangesAsync();
        _log.LogInformation("audit {Action} actor={Actor} target={Target}", action, actorId, targetId);
    }
}
