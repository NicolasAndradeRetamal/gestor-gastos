using FluentValidation;
using GestorGastos.Api.Dtos.Budgets;

namespace GestorGastos.Api.Validators;

public class BudgetCreateRequestValidator : AbstractValidator<BudgetCreateRequest>
{
    public BudgetCreateRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("La categoría es obligatoria.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El límite debe ser mayor que 0.");
    }
}

public class BudgetUpdateRequestValidator : AbstractValidator<BudgetUpdateRequest>
{
    public BudgetUpdateRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El límite debe ser mayor que 0.");
    }
}
