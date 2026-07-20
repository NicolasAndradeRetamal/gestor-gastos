namespace GestorGastos.Api.Dtos.Auth;

public record RegisterRequest(string Email, string Password, string DisplayName);

public record LoginRequest(string Email, string Password);

public record UserDto(Guid Id, string Email, string DisplayName, bool TwoFactorEnabled);

public record AuthResponse(
    string Token,
    DateTimeOffset ExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    UserDto User);

/// <summary>Returned by login when the account has 2FA enabled, in place of the tokens.</summary>
public record TwoFactorChallengeResponse(bool TwoFactorRequired, string TwoFactorToken);

public record RefreshRequest(string RefreshToken);

public record RefreshResponse(
    string Token,
    DateTimeOffset ExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);

public record LogoutRequest(string RefreshToken);
