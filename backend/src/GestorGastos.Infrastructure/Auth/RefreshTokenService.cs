using System.Security.Cryptography;
using GestorGastos.Domain.Common;
using GestorGastos.Domain.Entities;
using GestorGastos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GestorGastos.Infrastructure.Auth;

public interface IRefreshTokenService
{
    /// <summary>Issues a new refresh token for a session (new or continuing), persisting its hash.</summary>
    Task<IssuedRefreshToken> IssueAsync(Guid userId, Guid sessionId, string? ip, CancellationToken ct);

    /// <summary>
    /// Validates and rotates a presented refresh token. Presenting a revoked token
    /// revokes the whole session family (reuse detection). Throws on any invalid token.
    /// </summary>
    Task<RefreshRotation> RotateAsync(string presentedToken, string? ip, CancellationToken ct);

    /// <summary>Revokes the whole session family of a presented token (logout). Idempotent.</summary>
    Task RevokeFamilyAsync(string presentedToken, CancellationToken ct);
}

public record IssuedRefreshToken(string Token, DateTimeOffset ExpiresAt, Guid SessionId);

public record RefreshRotation(User User, IssuedRefreshToken Refresh);

public class RefreshTokenService(AppDbContext db, IOptions<JwtOptions> options) : IRefreshTokenService
{
    private static readonly string InvalidMessage = "La sesión expiró o no es válida. Inicia sesión de nuevo.";

    private readonly JwtOptions _options = options.Value;

    public async Task<IssuedRefreshToken> IssueAsync(Guid userId, Guid sessionId, string? ip, CancellationToken ct)
    {
        var (plain, entity) = CreateEntity(userId, sessionId, ip);
        db.RefreshTokens.Add(entity);
        await db.SaveChangesAsync(ct);

        return new IssuedRefreshToken(plain, entity.ExpiresAt, sessionId);
    }

    public async Task<RefreshRotation> RotateAsync(string presentedToken, string? ip, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var hash = TokenHashing.Hash(presentedToken);

        var current = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (current is null)
            throw new InvalidCredentialsException(InvalidMessage);

        // A revoked token presented again signals theft: kill the whole family.
        if (current.RevokedAt is not null)
        {
            await RevokeFamilyAsync(current.SessionId, now, ct);
            throw new InvalidCredentialsException(InvalidMessage);
        }

        if (current.ExpiresAt <= now)
            throw new InvalidCredentialsException(InvalidMessage);

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == current.UserId, ct)
            ?? throw new InvalidCredentialsException(InvalidMessage);

        var (plain, replacement) = CreateEntity(current.UserId, current.SessionId, ip);
        current.RevokedAt = now;
        current.ReplacedByTokenId = replacement.Id;
        db.RefreshTokens.Add(replacement);
        await db.SaveChangesAsync(ct);

        return new RefreshRotation(user, new IssuedRefreshToken(plain, replacement.ExpiresAt, current.SessionId));
    }

    public async Task RevokeFamilyAsync(string presentedToken, CancellationToken ct)
    {
        var hash = TokenHashing.Hash(presentedToken);
        var current = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (current is null)
            return;

        await RevokeFamilyAsync(current.SessionId, DateTimeOffset.UtcNow, ct);
    }

    private async Task RevokeFamilyAsync(Guid sessionId, DateTimeOffset now, CancellationToken ct)
    {
        var family = await db.RefreshTokens
            .Where(t => t.SessionId == sessionId && t.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in family)
            token.RevokedAt = now;

        if (family.Count > 0)
            await db.SaveChangesAsync(ct);
    }

    private (string Plain, RefreshToken Entity) CreateEntity(Guid userId, Guid sessionId, string? ip)
    {
        var plain = Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SessionId = sessionId,
            TokenHash = TokenHashing.Hash(plain),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenDays),
            CreatedByIp = ip,
        };

        return (plain, entity);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
