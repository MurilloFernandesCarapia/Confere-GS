using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Dtos.Direitos;
using FluentValidation;

namespace Confere.Radar.Application.Validators
{
    public class PerfilSocialRequestValidator : AbstractValidator<PerfilSocialRequest>
    {
        public PerfilSocialRequestValidator()
        {
            RuleFor(x => x.Idade)
                .InclusiveBetween(0, 130).When(x => x.Idade.HasValue).WithMessage("Informe uma idade entre 0 e 130 anos.");

            RuleFor(x => x.RendaPcFaixa)
                .Must(faixa => FaixaRenda.Todas.Contains(faixa!.Trim().ToUpperInvariant()))
                .When(x => !string.IsNullOrWhiteSpace(x.RendaPcFaixa))
                .WithMessage($"Faixa de renda inválida. Use uma destas: {string.Join(", ", FaixaRenda.Todas)}. A renda é sempre informada por faixa, nunca o valor exato.");

            RuleFor(x => x.QtdMoradores)
                .InclusiveBetween(1, 30).When(x => x.QtdMoradores.HasValue).WithMessage("Informe entre 1 e 30 moradores.");

            RuleFor(x => x.CodIbgeMunicipio)
                .InclusiveBetween(1000000, 9999999).When(x => x.CodIbgeMunicipio.HasValue).WithMessage("Informe o código IBGE do município (7 dígitos).");
        }
    }
}
