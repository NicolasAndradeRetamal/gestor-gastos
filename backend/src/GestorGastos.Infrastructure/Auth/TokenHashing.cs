using System.Security.Cryptography;
using System.Text;

namespace GestorGastos.Infrastructure.Auth;

/// <summary>
/// SHA-256 hashing for high-entropy secrets (refresh tokens, recovery codes). These
/// are random values, so a fast hash is safe — a slow password hash is unnecessary.
/// </summary>
public static class TokenHashing
{
    public static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
