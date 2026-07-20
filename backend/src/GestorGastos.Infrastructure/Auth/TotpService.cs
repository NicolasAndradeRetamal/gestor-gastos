using Microsoft.Extensions.Options;
using OtpNet;

namespace GestorGastos.Infrastructure.Auth;

public interface ITotpService
{
    /// <summary>Generates a new random base32 TOTP secret.</summary>
    string GenerateSecret();

    /// <summary>Builds the otpauth:// URI an authenticator app scans from a QR code.</summary>
    string BuildOtpauthUri(string accountEmail, string secretBase32);

    /// <summary>Verifies a 6-digit code against the secret, tolerating ±1 time step of clock skew.</summary>
    bool VerifyCode(string secretBase32, string code);
}

public class TotpService(IOptions<TotpOptions> options) : ITotpService
{
    private readonly TotpOptions _options = options.Value;

    public string GenerateSecret() => Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));

    public string BuildOtpauthUri(string accountEmail, string secretBase32)
    {
        var issuer = Uri.EscapeDataString(_options.Issuer);
        var label = Uri.EscapeDataString($"{_options.Issuer}:{accountEmail}");
        return $"otpauth://totp/{label}?secret={secretBase32}&issuer={issuer}&algorithm=SHA1&digits=6&period=30";
    }

    public bool VerifyCode(string secretBase32, string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var totp = new Totp(Base32Encoding.ToBytes(secretBase32));
        return totp.VerifyTotp(code.Trim(), out _, new VerificationWindow(previous: 1, future: 1));
    }
}
