using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Dtos.Radar;
using FluentValidation;

namespace Confere.Radar.Application.Validators
{
    public class DenunciaRequestValidator : AbstractValidator<DenunciaRequest>
    {
        public DenunciaRequestValidator()
        {
            RuleFor(x => x.Tipo)
                .NotEmpty().WithMessage("Informe o tipo do identificador.")
                .Must(tipo => TipoIdentificador.Todos.Contains(tipo.Trim().ToUpperInvariant()))
                .When(x => !string.IsNullOrWhiteSpace(x.Tipo), ApplyConditionTo.CurrentValidator)
                .WithMessage($"Tipo inválido. Use um destes: {string.Join(", ", TipoIdentificador.Todos)}.");

            RuleFor(x => x.Valor)
                .NotEmpty().WithMessage("Informe o número, link, chave Pix, e-mail ou perfil denunciado.")
                .MaximumLength(300).WithMessage("O valor pode ter no máximo 300 caracteres.");

            RuleFor(x => x.TipoGolpeCodigo)
                .NotEmpty().WithMessage("Informe o código do tipo de golpe (veja GET /radar/tipos-golpe).");

            RuleFor(x => x.CodIbgeMunicipio)
                .InclusiveBetween(1000000, 9999999).WithMessage("Informe o código IBGE do município (7 dígitos).");

            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage("A descrição pode ter no máximo 500 caracteres.");

            RuleFor(x => x.IdVerificacao)
                .GreaterThan(0).When(x => x.IdVerificacao.HasValue).WithMessage("O id da verificação precisa ser positivo.");
        }
    }
}
