namespace GestorGastos.Api.Dtos.Auth;

public record TwoFactorSetupResponse(string Secret, string OtpauthUri);

public record TwoFactorEnableRequest(string Code);

public record TwoFactorEnableResponse(IReadOnlyList<string> RecoveryCodes);

public record TwoFactorDisableRequest(string Password, string Code);

public record TwoFactorVerifyRequest(string TwoFactorToken, string Code);
