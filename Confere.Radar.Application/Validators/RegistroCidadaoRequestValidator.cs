using Confere.Radar.Domain.Dtos.Auth;
using FluentValidation;

namespace Confere.Radar.Application.Validators
{
    public class RegistroCidadaoRequestValidator : AbstractValidator<RegistroCidadaoRequest>
    {
        public RegistroCidadaoRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Informe o nome.")
                .MaximumLength(120).WithMessage("O nome pode ter no máximo 120 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Informe o e-mail.")
                .EmailAddress().WithMessage("E-mail inválido.")
                .MaximumLength(150).WithMessage("O e-mail pode ter no máximo 150 caracteres.");

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("Informe a senha.")
                .MinimumLength(8).WithMessage("A senha precisa ter pelo menos 8 caracteres.")
                .Matches("[A-Za-z]").WithMessage("A senha precisa ter pelo menos uma letra.")
                .Matches("[0-9]").WithMessage("A senha precisa ter pelo menos um número.");

            RuleFor(x => x.Telefone)
                .MaximumLength(20).WithMessage("O telefone pode ter no máximo 20 caracteres.");

            RuleFor(x => x.CodIbgeMunicipio)
                .InclusiveBetween(1000000, 9999999).WithMessage("Informe o código IBGE do município (7 dígitos).");
        }
    }
}
