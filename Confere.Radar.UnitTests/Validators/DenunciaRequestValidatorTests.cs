using Confere.Radar.Application.Validators;
using Confere.Radar.Domain.Dtos.Radar;

namespace Confere.Radar.UnitTests.Validators
{
    public class DenunciaRequestValidatorTests
    {
        private readonly DenunciaRequestValidator _validator = new();

        private static DenunciaRequest Valido() => new("TELEFONE", "11912345678", "FALSA_CENTRAL", 3550308, "Ligaram do banco", 981);

        [Fact]
        public void RequisicaoValida_NaoTemErros()
        {
            Assert.True(_validator.Validate(Valido()).IsValid);
        }

        [Fact]
        public void TipoEmMinusculo_EhValido()
        {
            Assert.True(_validator.Validate(Valido() with { Tipo = "link" }).IsValid);
        }

        [Fact]
        public void TipoInvalido_DaErroNoTipo()
        {
            var resultado = _validator.Validate(Valido() with { Tipo = "FAX" });

            Assert.Contains(resultado.Errors, e => e.PropertyName == "Tipo");
        }

        [Fact]
        public void CamposObrigatoriosVazios_DaUmErroPorCampo()
        {
            var resultado = _validator.Validate(new DenunciaRequest("", "", "", 0, null, null));

            Assert.Contains(resultado.Errors, e => e.PropertyName == "Tipo");
            Assert.Contains(resultado.Errors, e => e.PropertyName == "Valor");
            Assert.Contains(resultado.Errors, e => e.PropertyName == "TipoGolpeCodigo");
            Assert.Contains(resultado.Errors, e => e.PropertyName == "CodIbgeMunicipio");
        }

        [Fact]
        public void DescricaoMuitoLonga_DaErro()
        {
            var resultado = _validator.Validate(Valido() with { Descricao = new string('a', 501) });

            Assert.Contains(resultado.Errors, e => e.PropertyName == "Descricao");
        }

        [Fact]
        public void IdVerificacaoNegativo_DaErro()
        {
            var resultado = _validator.Validate(Valido() with { IdVerificacao = -1 });

            Assert.Contains(resultado.Errors, e => e.PropertyName == "IdVerificacao");
        }
    }
}
