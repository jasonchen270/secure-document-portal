using SecureDocumentPortal.Api.Domain;

namespace SecureDocumentPortal.Api.Auth;

public record AuthTokens(string AccessToken, string RefreshToken, DateTime AccessExpiresAt);

public interface ITokenService
{
    AuthTokens Issue(User user);
    string HashRefreshToken(string token);
    bool TryValidateAccessToken(string token, out Guid userId, out string role);
}
