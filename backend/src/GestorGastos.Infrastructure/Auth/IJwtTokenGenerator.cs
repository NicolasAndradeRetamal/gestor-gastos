using GestorGastos.Domain.Entities;

namespace GestorGastos.Infrastructure.Auth;

public interface IJwtTokenGenerator
{
    JwtToken Generate(User user);
}

public record JwtToken(string Value, DateTimeOffset ExpiresAt);
