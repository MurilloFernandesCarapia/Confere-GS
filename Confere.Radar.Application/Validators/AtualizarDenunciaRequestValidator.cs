using Confere.Radar.Domain.Dtos.Radar;
using FluentValidation;

namespace Confere.Radar.Application.Validators
{
    public class AtualizarDenunciaRequestValidator : AbstractValidator<AtualizarDenunciaRequest>
    {
        public AtualizarDenunciaRequestValidator()
        {
            RuleFor(x => x.TipoGolpeCodigo)
                .NotEmpty().WithMessage("Informe o código do tipo de golpe (veja GET /radar/tipos-golpe).");

            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage("A descrição pode ter no máximo 500 caracteres.");
        }
    }
}
