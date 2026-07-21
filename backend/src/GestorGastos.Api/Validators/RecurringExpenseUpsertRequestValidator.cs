using FluentValidation;
using GestorGastos.Api.Dtos.Recurring;

namespace GestorGastos.Api.Validators;

public class RecurringExpenseUpsertRequestValidator : AbstractValidator<RecurringExpenseUpsertRequest>
{
    public RecurringExpenseUpsertRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("La categoría es obligatoria.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor que 0.");

        RuleFor(x => x.DayOfMonth)
            .InclusiveBetween(1, 31).WithMessage("El día del mes debe estar entre 1 y 31.");

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("La nota no puede superar los 500 caracteres.");
    }
}
