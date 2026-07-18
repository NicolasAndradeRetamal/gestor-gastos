using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using GestorGastos.Domain.Entities;
using GestorGastos.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace GestorGastos.Tests.Unit.Auth;

public class JwtTokenGeneratorTests
{
    private readonly JwtTokenGenerator _generator = new(Options.Create(new JwtOptions
    {
        Secret = "unit-test-signing-key-at-least-32-characters-long",
        Issuer = "GestorGastosTests",
        Audience = "GestorGastosTests",
        ExpiryMinutes = 60,
    }));

    [Fact]
    public void Generate_IncludesRequiredClaims()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "ana@mail.com", DisplayName = "Ana" };

        var token = _generator.Generate(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.Value);
        jwt.Subject.Should().Be(user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
        jwt.Issuer.Should().Be("GestorGastosTests");
        jwt.Audiences.Should().Contain("GestorGastosTests");
    }

    [Fact]
    public void Generate_SetsExpiryAccordingToConfiguredMinutes()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "ana@mail.com", DisplayName = "Ana" };
        var before = DateTimeOffset.UtcNow;

        var token = _generator.Generate(user);

        token.ExpiresAt.Should().BeCloseTo(before.AddMinutes(60), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Generate_ProducesDifferentJtiOnEachCall()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "ana@mail.com", DisplayName = "Ana" };

        var token1 = _generator.Generate(user);
        var token2 = _generator.Generate(user);

        token1.Value.Should().NotBe(token2.Value);
    }
}
