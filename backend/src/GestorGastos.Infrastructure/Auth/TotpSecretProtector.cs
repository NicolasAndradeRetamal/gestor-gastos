using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace GestorGastos.Infrastructure.Auth;

public interface ITotpSecretProtector
{
    byte[] Protect(string plaintext);
    string Unprotect(byte[] payload);
}

/// <summary>
/// Encrypts TOTP secrets at rest with AES-256-GCM. The key lives in configuration
/// (environment), so decryption survives redeploys on ephemeral filesystems. Stored
/// payload layout: nonce (12 bytes) || ciphertext || tag (16 bytes).
/// </summary>
public class AesGcmTotpSecretProtector : ITotpSecretProtector
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _key;

    public AesGcmTotpSecretProtector(IOptions<TotpOptions> options)
    {
        var configured = options.Value.EncryptionKey;
        if (string.IsNullOrWhiteSpace(configured))
            throw new InvalidOperationException("Missing 'Totp:EncryptionKey' configuration.");

        _key = Convert.FromBase64String(configured);
        if (_key.Length != 32)
            throw new InvalidOperationException("'Totp:EncryptionKey' must decode to 32 bytes (AES-256).");
    }

    public byte[] Protect(string plaintext)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipher = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plainBytes, cipher, tag);

        var payload = new byte[NonceSize + cipher.Length + TagSize];
        Buffer.BlockCopy(nonce, 0, payload, 0, NonceSize);
        Buffer.BlockCopy(cipher, 0, payload, NonceSize, cipher.Length);
        Buffer.BlockCopy(tag, 0, payload, NonceSize + cipher.Length, TagSize);
        return payload;
    }

    public string Unprotect(byte[] payload)
    {
        if (payload.Length < NonceSize + TagSize)
            throw new ArgumentException("Invalid encrypted payload.", nameof(payload));

        var nonce = payload.AsSpan(0, NonceSize);
        var cipherLength = payload.Length - NonceSize - TagSize;
        var cipher = payload.AsSpan(NonceSize, cipherLength);
        var tag = payload.AsSpan(NonceSize + cipherLength, TagSize);
        var plainBytes = new byte[cipherLength];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, cipher, tag, plainBytes);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
