namespace GestorGastos.Infrastructure.Auth;

/// <summary>Bound from the "Jwt" configuration section; secret comes from environment/user-secrets.</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    // Access token lifetime. Short-lived; renewed via refresh tokens.
    public int ExpiryMinutes { get; set; } = 15;

    // Refresh token lifetime (sliding via rotation).
    public int RefreshTokenDays { get; set; } = 14;

    // Ephemeral 2FA challenge token lifetime and its dedicated audience, so the
    // normal Bearer scheme never accepts a challenge token as an access token.
    public int TwoFactorChallengeMinutes { get; set; } = 5;
    public string TwoFactorAudience { get; set; } = "gestor-gastos-2fa";
}
