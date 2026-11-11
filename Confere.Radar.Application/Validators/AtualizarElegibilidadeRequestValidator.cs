using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Dtos.Direitos;
using FluentValidation;

namespace Confere.Radar.Application.Validators
{
    public class AtualizarElegibilidadeRequestValidator : AbstractValidator<AtualizarElegibilidadeRequest>
    {
        private static readonly string[] StatusPermitidos = { StatusElegibilidade.Solicitado, StatusElegibilidade.Concedido };

        public AtualizarElegibilidadeRequestValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Informe o novo status.")
                .Must(status => StatusPermitidos.Contains(status.Trim().ToUpperInvariant()))
                .When(x => !string.IsNullOrWhiteSpace(x.Status))
                .WithMessage("Use SOLICITADO quando fizer o pedido ou CONCEDIDO quando receber o benefício.");
        }
    }
}
