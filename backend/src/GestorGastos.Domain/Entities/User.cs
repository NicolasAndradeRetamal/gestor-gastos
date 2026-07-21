using GestorGastos.Domain.Common;

namespace GestorGastos.Domain.Entities;

public class User : AuditableEntity
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan CountingWindow = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    // Account lockout (brute-force protection).
    public int FailedLoginAttempts { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public DateTimeOffset? LastFailedLoginAt { get; set; }

    // TOTP two-factor authentication.
    public bool TwoFactorEnabled { get; set; }
    // Shared TOTP secret encrypted at rest (AES-256-GCM); never the plain base32.
    public byte[]? TwoFactorSecret { get; set; }
    public DateTimeOffset? TwoFactorEnabledAt { get; set; }

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<TwoFactorRecoveryCode> RecoveryCodes { get; set; } = new List<TwoFactorRecoveryCode>();

    public bool IsLockedOut(DateTimeOffset now) => LockedUntil.HasValue && LockedUntil.Value > now;

    /// <summary>
    /// Records a failed login attempt with a sliding counting window; locks the
    /// account temporarily once the threshold is reached.
    /// </summary>
    public void RegisterFailedLogin(DateTimeOffset now)
    {
        if (LastFailedLoginAt is null || now - LastFailedLoginAt.Value > CountingWindow)
            FailedLoginAttempts = 0;

        FailedLoginAttempts++;
        LastFailedLoginAt = now;

        if (FailedLoginAttempts >= MaxFailedAttempts)
        {
            LockedUntil = now + LockoutDuration;
            FailedLoginAttempts = 0;
        }
    }

    /// <summary>Clears the lockout state after a successful authentication.</summary>
    public void RegisterSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
        LastFailedLoginAt = null;
    }
}
