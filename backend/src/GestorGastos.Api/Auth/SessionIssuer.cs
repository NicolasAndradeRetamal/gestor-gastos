using GestorGastos.Api.Dtos.Auth;
using GestorGastos.Api.Mapping;
using GestorGastos.Domain.Entities;
using GestorGastos.Infrastructure.Auth;

namespace GestorGastos.Api.Auth;

/// <summary>Issues a full session (access token + a fresh refresh-token family) for a user.</summary>
public class SessionIssuer(IJwtTokenGenerator jwt, IRefreshTokenService refreshTokens)
{
    public async Task<AuthResponse> IssueAsync(User user, string? ip, CancellationToken ct)
    {
        var access = jwt.Generate(user);
        var refresh = await refreshTokens.IssueAsync(user.Id, Guid.NewGuid(), ip, ct);
        return new AuthResponse(access.Value, access.ExpiresAt, refresh.Token, refresh.ExpiresAt, user.ToDto());
    }
}
