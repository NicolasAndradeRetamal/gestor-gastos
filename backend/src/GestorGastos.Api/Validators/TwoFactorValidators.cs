using FluentValidation;
using GestorGastos.Api.Dtos.Auth;

namespace GestorGastos.Api.Validators;

public class TwoFactorEnableRequestValidator : AbstractValidator<TwoFactorEnableRequest>
{
    public TwoFactorEnableRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es obligatorio.")
            .Matches("^[0-9]{6}$").WithMessage("El código debe tener 6 dígitos.");
    }
}

public class TwoFactorVerifyRequestValidator : AbstractValidator<TwoFactorVerifyRequest>
{
    public TwoFactorVerifyRequestValidator()
    {
        RuleFor(x => x.TwoFactorToken)
            .NotEmpty().WithMessage("El desafío es obligatorio.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es obligatorio.");
    }
}

public class TwoFactorDisableRequestValidator : AbstractValidator<TwoFactorDisableRequest>
{
    public TwoFactorDisableRequestValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es obligatorio.");
    }
}
