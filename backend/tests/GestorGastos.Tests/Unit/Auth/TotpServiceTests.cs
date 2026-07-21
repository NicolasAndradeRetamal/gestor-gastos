using FluentAssertions;
using GestorGastos.Infrastructure.Auth;
using Microsoft.Extensions.Options;
using OtpNet;

namespace GestorGastos.Tests.Unit.Auth;

public class TotpServiceTests
{
    private readonly TotpService _service = new(Options.Create(new TotpOptions { Issuer = "GestorGastos" }));

    [Fact]
    public void GenerateSecret_ProducesUsableBase32()
    {
        var secret = _service.GenerateSecret();

        secret.Should().NotBeNullOrWhiteSpace();
        var act = () => Base32Encoding.ToBytes(secret);
        act.Should().NotThrow();
    }

    [Fact]
    public void VerifyCode_WithCurrentCode_ReturnsTrue()
    {
        var secret = _service.GenerateSecret();
        var currentCode = new Totp(Base32Encoding.ToBytes(secret)).ComputeTotp();

        _service.VerifyCode(secret, currentCode).Should().BeTrue();
    }

    [Fact]
    public void VerifyCode_WithWrongCode_ReturnsFalse()
    {
        var secret = _service.GenerateSecret();

        _service.VerifyCode(secret, "000000").Should().BeFalse();
    }

    [Fact]
    public void BuildOtpauthUri_ContainsIssuerAndSecret()
    {
        var uri = _service.BuildOtpauthUri("ana@mail.com", "JBSWY3DPEHPK3PXP");

        uri.Should().StartWith("otpauth://totp/");
        uri.Should().Contain("secret=JBSWY3DPEHPK3PXP");
        uri.Should().Contain("issuer=GestorGastos");
        uri.Should().Contain("algorithm=SHA1");
    }
}
