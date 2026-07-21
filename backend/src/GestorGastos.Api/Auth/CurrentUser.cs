using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GestorGastos.Api.Auth;

public static class CurrentUser
{
    public static Guid GetId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (sub is null || !Guid.TryParse(sub, out var id))
            throw new InvalidOperationException("The authenticated principal has no valid 'sub' claim.");

        return id;
    }

    /// <summary>Client IP (honoring forwarded headers behind the platform proxy), for token audit and partitioning.</summary>
    public static string? GetClientIp(this HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString();
}
