using GestorGastos.Domain.Common;

namespace GestorGastos.Domain.Entities;

/// <summary>
/// Opaque refresh token, stored only as a SHA-256 hash. Tokens are grouped into a
/// session family (<see cref="SessionId"/>) and rotated on every use; presenting a
/// revoked token signals theft and revokes the whole family.
/// </summary>
public class RefreshToken : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; }
    public string? CreatedByIp { get; set; }

    public User? User { get; set; }

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;
}
