using FluentValidation;
using GestorGastos.Api.Dtos.Expenses;

namespace GestorGastos.Api.Validators;

public class ExpenseListQueryValidator : AbstractValidator<ExpenseListQuery>
{
    private static readonly string[] AllowedSorts = ["spentAt", "-spentAt", "amount", "-amount"];

    public ExpenseListQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("La página debe ser mayor o igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100.");

        RuleFor(x => x.Sort)
            .Must(sort => AllowedSorts.Contains(sort))
            .WithMessage("El campo de orden debe ser 'spentAt' o 'amount', con prefijo '-' opcional.");

        RuleFor(x => x)
            .Must(x => x.From is null || x.To is null || x.From <= x.To)
            .WithName("from")
            .WithMessage("La fecha 'from' no puede ser posterior a 'to'.");
    }
}
