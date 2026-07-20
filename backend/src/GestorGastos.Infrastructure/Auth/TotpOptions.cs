namespace GestorGastos.Infrastructure.Auth;

/// <summary>Bound from the "Totp" configuration section; the encryption key comes from environment/user-secrets.</summary>
public class TotpOptions
{
    public const string SectionName = "Totp";

    // Base64-encoded 32-byte key for AES-256-GCM encryption of TOTP secrets at rest.
    public string EncryptionKey { get; set; } = string.Empty;

    // Issuer label shown in authenticator apps (otpauth:// URI).
    public string Issuer { get; set; } = "GestorGastos";
}
