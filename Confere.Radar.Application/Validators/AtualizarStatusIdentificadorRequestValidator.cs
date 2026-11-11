using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Dtos.Gestao;
using FluentValidation;

namespace Confere.Radar.Application.Validators
{
    public class AtualizarStatusIdentificadorRequestValidator : AbstractValidator<AtualizarStatusIdentificadorRequest>
    {
        private static readonly string[] StatusPermitidos = { StatusIdentificador.Descartado, StatusIdentificador.EmAnalise };

        public AtualizarStatusIdentificadorRequestValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Informe o novo status.")
                .Must(status => StatusPermitidos.Contains(status.Trim().ToUpperInvariant()))
                .When(x => !string.IsNullOrWhiteSpace(x.Status), ApplyConditionTo.CurrentValidator)
                .WithMessage("Use DESCARTADO para marcar falso positivo ou EM_ANALISE para reabrir. CONFIRMADO só é definido pelo score (RN13).");
        }
    }
}
