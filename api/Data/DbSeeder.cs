using BCrypt.Net;
using SecureDocumentPortal.Api.Auth;
using SecureDocumentPortal.Api.Domain;

namespace SecureDocumentPortal.Api.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var admin = new User
        {
            Email = "admin@portal.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ChangeMe!123"),
            Role = Roles.Admin
        };
        var reviewer = new User
        {
            Email = "reviewer@portal.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ChangeMe!123"),
            Role = Roles.Reviewer
        };
        var uploader = new User
        {
            Email = "uploader@portal.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ChangeMe!123"),
            Role = Roles.Uploader
        };
        db.Users.AddRange(admin, reviewer, uploader);
        db.SaveChanges();
    }
}
