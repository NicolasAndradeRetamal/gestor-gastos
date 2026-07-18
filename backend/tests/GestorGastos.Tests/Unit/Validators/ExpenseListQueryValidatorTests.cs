using FluentAssertions;
using GestorGastos.Api.Dtos.Expenses;
using GestorGastos.Api.Validators;

namespace GestorGastos.Tests.Unit.Validators;

public class ExpenseListQueryValidatorTests
{
    private readonly ExpenseListQueryValidator _validator = new();

    [Fact]
    public void Validate_WithDefaults_Passes()
    {
        var result = _validator.Validate(new ExpenseListQuery());

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("spentAt")]
    [InlineData("-spentAt")]
    [InlineData("amount")]
    [InlineData("-amount")]
    public void Validate_WithAllowedSort_Passes(string sort)
    {
        var result = _validator.Validate(new ExpenseListQuery { Sort = sort });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithUnknownSort_Fails()
    {
        var result = _validator.Validate(new ExpenseListQuery { Sort = "category" });

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithPageSizeOverMax_Fails()
    {
        var result = _validator.Validate(new ExpenseListQuery { PageSize = 101 });

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithPageBelowOne_Fails()
    {
        var result = _validator.Validate(new ExpenseListQuery { Page = 0 });

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithFromAfterTo_Fails()
    {
        var result = _validator.Validate(new ExpenseListQuery
        {
            From = new DateOnly(2026, 7, 1),
            To = new DateOnly(2026, 6, 1),
        });

        result.IsValid.Should().BeFalse();
    }
}
