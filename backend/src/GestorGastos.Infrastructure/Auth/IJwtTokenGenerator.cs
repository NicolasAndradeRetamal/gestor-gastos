using GestorGastos.Domain.Entities;

namespace GestorGastos.Infrastructure.Auth;

public interface IJwtTokenGenerator
{
    JwtToken Generate(User user);

    /// <summary>Short-lived token proving password success while a second factor is pending.</summary>
    JwtToken GenerateTwoFactorChallenge(User user);

    /// <summary>Validates a 2FA challenge token; returns the user id, or null if invalid/expired.</summary>
    Guid? ValidateTwoFactorChallenge(string token);
}

public record JwtToken(string Value, DateTimeOffset ExpiresAt);
