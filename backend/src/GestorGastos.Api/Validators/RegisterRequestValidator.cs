using FluentValidation;
using GestorGastos.Api.Dtos.Auth;

namespace GestorGastos.Api.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .MaximumLength(320).WithMessage("El correo no puede superar los 320 caracteres.")
            .EmailAddress().WithMessage("El correo no tiene un formato válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .Length(8, 100).WithMessage("La contraseña debe tener entre 8 y 100 caracteres.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .Length(1, 100).WithMessage("El nombre debe tener entre 1 y 100 caracteres.");
    }
}
