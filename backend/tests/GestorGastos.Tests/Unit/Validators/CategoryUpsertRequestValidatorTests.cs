using FluentAssertions;
using GestorGastos.Api.Dtos.Categories;
using GestorGastos.Api.Validators;

namespace GestorGastos.Tests.Unit.Validators;

public class CategoryUpsertRequestValidatorTests
{
    private readonly CategoryUpsertRequestValidator _validator = new();

    [Theory]
    [InlineData("#F97316")]
    [InlineData("#000000")]
    [InlineData("#ffffff")]
    public void Validate_WithValidHexColor_Passes(string color)
    {
        var result = _validator.Validate(new CategoryUpsertRequest("Comida", color, null));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("F97316")]
    [InlineData("#FFF")]
    [InlineData("red")]
    [InlineData("")]
    public void Validate_WithInvalidColor_Fails(string color)
    {
        var result = _validator.Validate(new CategoryUpsertRequest("Comida", color, null));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithNameTooLong_Fails()
    {
        var result = _validator.Validate(new CategoryUpsertRequest(new string('a', 61), "#F97316", null));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithEmptyName_Fails()
    {
        var result = _validator.Validate(new CategoryUpsertRequest("", "#F97316", null));

        result.IsValid.Should().BeFalse();
    }
}
