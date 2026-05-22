using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SecureDocumentPortal.Api.Domain;

namespace SecureDocumentPortal.Api.Auth;

public class TokenService : ITokenService
{
    private readonly IConfiguration _cfg;
    private readonly SymmetricSecurityKey _key;
    private readonly TimeSpan _accessLifetime;

    public TokenService(IConfiguration cfg)
    {
        _cfg = cfg;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Secret"]!));
        _accessLifetime = TimeSpan.FromMinutes(int.Parse(cfg["Jwt:AccessMinutes"] ?? "15"));
    }

    public AuthTokens Issue(User user)
    {
        var now = DateTime.UtcNow;
        var expires = now.Add(_accessLifetime);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var jwt = new JwtSecurityToken(
            issuer: _cfg["Jwt:Issuer"],
            audience: _cfg["Jwt:Audience"],
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.HmacSha256));

        var access = new JwtSecurityTokenHandler().WriteToken(jwt);
        var refresh = GenerateRefreshToken();
        return new AuthTokens(access, refresh, expires);
    }

    public string HashRefreshToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    public bool TryValidateAccessToken(string token, out Guid userId, out string role)
    {
        userId = Guid.Empty;
        role = string.Empty;
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidIssuer = _cfg["Jwt:Issuer"],
                ValidAudience = _cfg["Jwt:Audience"],
                IssuerSigningKey = _key,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            }, out _);

            var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            role = principal.FindFirstValue(ClaimTypes.Role) ?? "";
            return Guid.TryParse(sub, out userId);
        }
        catch
        {
            return false;
        }
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
