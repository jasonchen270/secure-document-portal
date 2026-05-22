namespace SecureDocumentPortal.Api.Domain;

public class AuditEvent
{
    public long Id { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public Guid? ActorId { get; set; }
    public string Action { get; set; } = default!;
    public string? TargetType { get; set; }
    public string? TargetId { get; set; }
    public string? IpAddress { get; set; }
    public string? Metadata { get; set; }
}
