using FluentValidation;
using GestorGastos.Api.Dtos.Categories;

namespace GestorGastos.Api.Validators;

public class CategoryUpsertRequestValidator : AbstractValidator<CategoryUpsertRequest>
{
    public CategoryUpsertRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(60).WithMessage("El nombre no puede superar los 60 caracteres.");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("El color es obligatorio.")
            .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("El color debe tener el formato hexadecimal #RRGGBB.");

        RuleFor(x => x.Icon)
            .MaximumLength(40).WithMessage("El icono no puede superar los 40 caracteres.");
    }
}
