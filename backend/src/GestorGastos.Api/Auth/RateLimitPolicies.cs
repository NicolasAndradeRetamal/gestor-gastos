namespace GestorGastos.Api.Auth;

/// <summary>Named rate-limiting policies applied to the auth endpoints.</summary>
public static class RateLimitPolicies
{
    public const string Register = "auth-register";
    public const string Login = "auth-login";
    public const string Refresh = "auth-refresh";
    public const string TwoFactor = "auth-2fa";
}
