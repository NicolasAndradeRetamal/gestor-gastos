using FluentAssertions;
using GestorGastos.Infrastructure.Auth;

namespace GestorGastos.Tests.Unit.Auth;

public class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_NeverReturnsThePlainPassword()
    {
        var hash = _hasher.Hash("Password123");

        hash.Should().NotBe("Password123");
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("Password123");

        _hasher.Verify("Password123", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("Password123");

        _hasher.Verify("WrongPassword", hash).Should().BeFalse();
    }

    [Fact]
    public void Hash_IsSaltedSoTwoHashesOfSamePasswordDiffer()
    {
        var hash1 = _hasher.Hash("Password123");
        var hash2 = _hasher.Hash("Password123");

        hash1.Should().NotBe(hash2);
    }
}
