using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Dtos.AlertasRegionais;
using FluentValidation;

namespace Confere.Radar.Application.Validators
{
    public class AlertaRegionalRequestValidator : AbstractValidator<AlertaRegionalRequest>
    {
        public AlertaRegionalRequestValidator()
        {
            RuleFor(x => x.CodIbgeMunicipio)
                .InclusiveBetween(1000000, 9999999).WithMessage("Informe o código IBGE do município (7 dígitos).");

            RuleFor(x => x.TipoGolpeCodigo)
                .NotEmpty().WithMessage("Informe o código do tipo de golpe (veja GET /radar/tipos-golpe).");

            RuleFor(x => x.Titulo)
                .NotEmpty().WithMessage("Informe o título do alerta.")
                .MaximumLength(120).WithMessage("O título pode ter no máximo 120 caracteres.");

            RuleFor(x => x.Mensagem)
                .NotEmpty().WithMessage("Informe a mensagem do alerta.")
                .MaximumLength(600).WithMessage("A mensagem pode ter no máximo 600 caracteres.");

            RuleFor(x => x.Nivel)
                .NotEmpty().WithMessage("Informe o nível do alerta.")
                .Must(nivel => NivelAlerta.Todos.Contains(nivel.Trim().ToUpperInvariant()))
                .When(x => !string.IsNullOrWhiteSpace(x.Nivel), ApplyConditionTo.CurrentValidator)
                .WithMessage($"Nível inválido. Use um destes: {string.Join(", ", NivelAlerta.Todos)}.");

            RuleFor(x => x.DtFim)
                .NotEmpty().WithMessage("Informe a data de fim do alerta.")
                .GreaterThan(x => x.DtInicio!.Value).When(x => x.DtInicio.HasValue, ApplyConditionTo.CurrentValidator)
                .WithMessage("A data de fim precisa ser depois da data de início.");
        }
    }
}
