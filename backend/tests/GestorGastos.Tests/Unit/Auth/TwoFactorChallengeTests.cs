using FluentAssertions;
using GestorGastos.Domain.Entities;
using GestorGastos.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace GestorGastos.Tests.Unit.Auth;

public class TwoFactorChallengeTests
{
    private readonly JwtTokenGenerator _generator = new(Options.Create(new JwtOptions
    {
        Secret = "unit-test-signing-key-at-least-32-characters-long",
        Issuer = "GestorGastosTests",
        Audience = "GestorGastosTests",
        TwoFactorAudience = "gestor-gastos-2fa",
        TwoFactorChallengeMinutes = 5,
    }));

    [Fact]
    public void ValidateTwoFactorChallenge_WithFreshChallenge_ReturnsUserId()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "ana@mail.com" };
        var challenge = _generator.GenerateTwoFactorChallenge(user);

        _generator.ValidateTwoFactorChallenge(challenge.Value).Should().Be(user.Id);
    }

    [Fact]
    public void ValidateTwoFactorChallenge_WithAccessToken_ReturnsNull()
    {
        // An access token has the app audience, not the 2FA audience, so it must
        // never pass as a challenge token.
        var user = new User { Id = Guid.NewGuid(), Email = "ana@mail.com" };
        var access = _generator.Generate(user);

        _generator.ValidateTwoFactorChallenge(access.Value).Should().BeNull();
    }

    [Fact]
    public void ValidateTwoFactorChallenge_WithGarbage_ReturnsNull()
    {
        _generator.ValidateTwoFactorChallenge("not-a-token").Should().BeNull();
    }
}
