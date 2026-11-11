using Confere.Radar.Application.Validators;
using Confere.Radar.Domain.Dtos.AlertasRegionais;

namespace Confere.Radar.UnitTests.Validators
{
    public class AlertaRegionalRequestValidatorTests
    {
        private readonly AlertaRegionalRequestValidator _validator = new();
        private readonly AtualizarAlertaRegionalRequestValidator _validatorAtualizar = new();

        private static AlertaRegionalRequest Valido() => new(3550308, "FALSA_PROVA_VIDA", "Golpe da prova de vida",
            "O INSS não pede selfie por WhatsApp.", "URGENTE", null, new DateTimeOffset(2026, 11, 25, 23, 59, 0, TimeSpan.FromHours(-3)));

        [Fact]
        public void ExemploDoContrato_EhValido()
        {
            Assert.True(_validator.Validate(Valido()).IsValid);
        }

        [Fact]
        public void NivelInvalido_DaErro()
        {
            Assert.Contains(_validator.Validate(Valido() with { Nivel = "ALTO" }).Errors, e => e.PropertyName == "Nivel");
        }

        [Fact]
        public void FimAntesDoInicio_DaErro()
        {
            var request = Valido() with { DtInicio = new DateTimeOffset(2026, 11, 26, 0, 0, 0, TimeSpan.FromHours(-3)) };

            Assert.Contains(_validator.Validate(request).Errors, e => e.PropertyName == "DtFim");
        }

        [Fact]
        public void SemDataFimESemDataInicio_DaErro()
        {
            Assert.Contains(_validator.Validate(Valido() with { DtFim = default }).Errors, e => e.PropertyName == "DtFim");
        }

        [Fact]
        public void NivelVazio_DaErro()
        {
            Assert.Contains(_validator.Validate(Valido() with { Nivel = "" }).Errors, e => e.PropertyName == "Nivel");
        }

        [Fact]
        public void TituloEMensagemVazios_DaErro()
        {
            var erros = _validator.Validate(Valido() with { Titulo = "", Mensagem = "" }).Errors;

            Assert.Contains(erros, e => e.PropertyName == "Titulo");
            Assert.Contains(erros, e => e.PropertyName == "Mensagem");
        }

        [Fact]
        public void Atualizar_SemDataFim_DaErro()
        {
            var request = new AtualizarAlertaRegionalRequest("FALSA_CENTRAL", "Título", "Mensagem", "INFO", default);

            Assert.Contains(_validatorAtualizar.Validate(request).Errors, e => e.PropertyName == "DtFim");
        }
    }
}
