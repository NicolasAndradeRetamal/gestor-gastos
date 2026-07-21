using FluentAssertions;
using GestorGastos.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace GestorGastos.Tests.Unit.Auth;

public class TotpSecretProtectorTests
{
    private static AesGcmTotpSecretProtector CreateProtector()
    {
        var key = Convert.ToBase64String(new byte[32]);
        return new AesGcmTotpSecretProtector(Options.Create(new TotpOptions { EncryptionKey = key }));
    }

    [Fact]
    public void ProtectThenUnprotect_RoundTripsTheSecret()
    {
        var protector = CreateProtector();

        var payload = protector.Protect("JBSWY3DPEHPK3PXP");

        protector.Unprotect(payload).Should().Be("JBSWY3DPEHPK3PXP");
    }

    [Fact]
    public void Protect_ProducesDifferentCiphertextEachTime()
    {
        var protector = CreateProtector();

        var first = protector.Protect("JBSWY3DPEHPK3PXP");
        var second = protector.Protect("JBSWY3DPEHPK3PXP");

        first.Should().NotEqual(second);
    }

    [Fact]
    public void Unprotect_WithTamperedPayload_Throws()
    {
        var protector = CreateProtector();
        var payload = protector.Protect("JBSWY3DPEHPK3PXP");
        payload[^1] ^= 0xFF;

        var act = () => protector.Unprotect(payload);

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Constructor_WithInvalidKeyLength_Throws()
    {
        var shortKey = Convert.ToBase64String(new byte[16]);

        var act = () => new AesGcmTotpSecretProtector(Options.Create(new TotpOptions { EncryptionKey = shortKey }));

        act.Should().Throw<InvalidOperationException>();
    }
}
