using SecureDocumentPortal.Api.Domain;

namespace SecureDocumentPortal.Api.Services;

public interface IUserService
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByIdAsync(Guid id);
    Task<User> CreateAsync(string email, string password, string role);
    Task<bool> VerifyPasswordAsync(User user, string password);
    Task<List<User>> ListAsync();
    Task UpdateLastLoginAsync(User user);
}
