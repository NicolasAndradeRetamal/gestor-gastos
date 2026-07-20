using GestorGastos.Domain.Common;

namespace GestorGastos.Domain.Entities;

/// <summary>Single-use recovery code (stored as a SHA-256 hash) for 2FA account recovery.</summary>
public class TwoFactorRecoveryCode : AuditableEntity
{
    public Guid UserId { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public DateTimeOffset? UsedAt { get; set; }

    public User? User { get; set; }
}
