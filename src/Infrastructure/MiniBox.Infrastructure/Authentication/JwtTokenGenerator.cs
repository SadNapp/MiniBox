using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MiniBox.Infrastructure.Authentication;

public sealed class JwtTokenGenerator(string signingKey, string issuer = "MiniBox", string audience = "MiniBox.Client")
{
    public (string Token, DateTime ExpiresAtUtc) GenerateRefreshToken()
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return (token, DateTime.UtcNow.AddDays(30));
    }

    public (string Token, DateTime ExpiresAtUtc) Generate(Guid userId, string email, string role = "Customer")
    {
        if (string.IsNullOrWhiteSpace(signingKey) || signingKey.Length < 32)
            throw new InvalidOperationException("JWT signing key must contain at least 32 characters.");
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddHours(1);
        var header = Encode(JsonSerializer.SerializeToUtf8Bytes(new { alg = "HS256", typ = "JWT" }));
        var payload = Encode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            sub = userId.ToString(), email, role, iss = issuer, aud = audience,
            iat = now.ToUnixTimeSeconds(), exp = expires.ToUnixTimeSeconds()
        }));
        var unsigned = $"{header}.{payload}";
        var signature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(signingKey), Encoding.ASCII.GetBytes(unsigned));
        return ($"{unsigned}.{Encode(signature)}", expires.UtcDateTime);
    }

    private static string Encode(byte[] value) => Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
