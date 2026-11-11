using Confere.Radar.Domain.Dtos.Auth;
using FluentValidation;

namespace Confere.Radar.Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Informe o e-mail.")
                .EmailAddress().WithMessage("E-mail inválido.")
                .MaximumLength(150).WithMessage("O e-mail pode ter no máximo 150 caracteres.");

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("Informe a senha.");
        }
    }
}
