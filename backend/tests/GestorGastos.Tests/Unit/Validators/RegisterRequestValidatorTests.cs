using FluentAssertions;
using GestorGastos.Api.Dtos.Auth;
using GestorGastos.Api.Validators;

namespace GestorGastos.Tests.Unit.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_Passes()
    {
        var result = _validator.Validate(new RegisterRequest("ana@mail.com", "secret123", "Ana"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "secret123", "Ana")]
    [InlineData("not-an-email", "secret123", "Ana")]
    [InlineData("ana@mail.com", "short", "Ana")]
    [InlineData("ana@mail.com", "secret123", "")]
    public void Validate_WithInvalidData_Fails(string email, string password, string displayName)
    {
        var result = _validator.Validate(new RegisterRequest(email, password, displayName));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithPasswordTooLong_Fails()
    {
        var result = _validator.Validate(new RegisterRequest("ana@mail.com", new string('a', 101), "Ana"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
