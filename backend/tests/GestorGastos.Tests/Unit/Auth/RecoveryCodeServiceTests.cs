using FluentAssertions;
using GestorGastos.Infrastructure.Auth;

namespace GestorGastos.Tests.Unit.Auth;

public class RecoveryCodeServiceTests
{
    private readonly RecoveryCodeService _service = new();

    [Fact]
    public void Generate_ProducesRequestedCountOfFormattedCodes()
    {
        var codes = _service.Generate(10);

        codes.Should().HaveCount(10);
        codes.Should().OnlyContain(c => c.PlainText.Length == 11 && c.PlainText[5] == '-');
        codes.Select(c => c.PlainText).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void HashInput_MatchesGeneratedHash_IgnoringCaseAndDashes()
    {
        var code = _service.Generate(1)[0];

        // Same code entered lowercase and without the dash must hash identically.
        var normalizedEntry = code.PlainText.Replace("-", string.Empty).ToLowerInvariant();

        _service.HashInput(normalizedEntry).Should().Be(code.Hash);
    }

    [Fact]
    public void HashInput_DiffersForDifferentCodes()
    {
        _service.HashInput("ABCDE-FGHJK").Should().NotBe(_service.HashInput("MNPQR-STUVW"));
    }
}
