using FluentValidation;
using GestorGastos.Api.Dtos.Expenses;

namespace GestorGastos.Api.Validators;

public class ExpenseUpsertRequestValidator : AbstractValidator<ExpenseUpsertRequest>
{
    public ExpenseUpsertRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor que 0.")
            .Must(HaveAtMostTwoDecimals).WithMessage("El monto admite como máximo 2 decimales.");

        RuleFor(x => x.SpentAt)
            .NotEmpty().WithMessage("La fecha es obligatoria.")
            .Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha no puede ser posterior a hoy.");

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("La nota no puede superar los 500 caracteres.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("La categoría es obligatoria.");
    }

    private static bool HaveAtMostTwoDecimals(decimal amount) => decimal.Round(amount, 2) == amount;
}
