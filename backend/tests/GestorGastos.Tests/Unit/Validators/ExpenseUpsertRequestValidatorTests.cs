using FluentAssertions;
using GestorGastos.Api.Dtos.Expenses;
using GestorGastos.Api.Validators;

namespace GestorGastos.Tests.Unit.Validators;

public class ExpenseUpsertRequestValidatorTests
{
    private readonly ExpenseUpsertRequestValidator _validator = new();
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public void Validate_WithValidData_Passes()
    {
        var result = _validator.Validate(new ExpenseUpsertRequest(42.50m, Today, "Almuerzo", Guid.NewGuid()));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_WithNonPositiveAmount_Fails(decimal amount)
    {
        var result = _validator.Validate(new ExpenseUpsertRequest(amount, Today, null, Guid.NewGuid()));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Amount");
    }

    [Fact]
    public void Validate_WithMoreThanTwoDecimals_Fails()
    {
        var result = _validator.Validate(new ExpenseUpsertRequest(10.123m, Today, null, Guid.NewGuid()));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithFutureDate_Fails()
    {
        var result = _validator.Validate(new ExpenseUpsertRequest(10m, Today.AddDays(1), null, Guid.NewGuid()));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SpentAt");
    }

    [Fact]
    public void Validate_WithEmptyCategoryId_Fails()
    {
        var result = _validator.Validate(new ExpenseUpsertRequest(10m, Today, null, Guid.Empty));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithNoteTooLong_Fails()
    {
        var result = _validator.Validate(new ExpenseUpsertRequest(10m, Today, new string('a', 501), Guid.NewGuid()));

        result.IsValid.Should().BeFalse();
    }
}
