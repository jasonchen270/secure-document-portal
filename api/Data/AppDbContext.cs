using Microsoft.EntityFrameworkCore;
using SecureDocumentPortal.Api.Domain;

namespace SecureDocumentPortal.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Role).HasMaxLength(32).IsRequired();
        });

        b.Entity<Document>(e =>
        {
            e.HasIndex(d => d.OwnerId);
            e.Property(d => d.Title).HasMaxLength(256).IsRequired();
            e.Property(d => d.Classification).HasMaxLength(32).IsRequired();
            e.HasOne(d => d.Owner).WithMany().HasForeignKey(d => d.OwnerId);
        });

        b.Entity<DocumentVersion>(e =>
        {
            e.HasIndex(v => new { v.DocumentId, v.Version }).IsUnique();
            e.Property(v => v.BlobUri).IsRequired();
            e.Property(v => v.Sha256).HasMaxLength(64).IsRequired();
            e.HasOne(v => v.Document).WithMany(d => d.Versions).HasForeignKey(v => v.DocumentId);
        });

        b.Entity<AuditEvent>(e =>
        {
            e.HasIndex(a => a.OccurredAt);
            e.HasIndex(a => a.ActorId);
            e.Property(a => a.Action).HasMaxLength(64).IsRequired();
        });

        b.Entity<RefreshToken>(e =>
        {
            e.HasIndex(r => r.TokenHash).IsUnique();
            e.Property(r => r.TokenHash).HasMaxLength(128).IsRequired();
        });
    }
}
