using Microsoft.EntityFrameworkCore;
using SecureDocumentPortal.Api.Data;
using SecureDocumentPortal.Api.Domain;

namespace SecureDocumentPortal.Api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    public UserService(AppDbContext db) => _db = db;

    public Task<User?> FindByEmailAsync(string email) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task<User?> FindByIdAsync(Guid id) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User> CreateAsync(string email, string password, string role)
    {
        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public Task<bool> VerifyPasswordAsync(User user, string password) =>
        Task.FromResult(BCrypt.Net.BCrypt.Verify(password, user.PasswordHash));

    public Task<List<User>> ListAsync() =>
        _db.Users.OrderBy(u => u.Email).ToListAsync();

    public async Task UpdateLastLoginAsync(User user)
    {
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
