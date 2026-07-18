using FluentAssertions;
using GestorGastos.Api.Dtos.Auth;
using GestorGastos.Api.Validators;

namespace GestorGastos.Tests.Unit.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_Passes()
    {
        var result = _validator.Validate(new LoginRequest("ana@mail.com", "anything"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyPassword_Fails()
    {
        var result = _validator.Validate(new LoginRequest("ana@mail.com", ""));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithInvalidEmail_Fails()
    {
        var result = _validator.Validate(new LoginRequest("not-an-email", "anything"));

        result.IsValid.Should().BeFalse();
    }
}
