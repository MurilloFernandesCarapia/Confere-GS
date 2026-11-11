using Confere.Radar.Application.Validators;
using Confere.Radar.Domain.Dtos.Direitos;
using Confere.Radar.Domain.Dtos.Gestao;
using Confere.Radar.Domain.Dtos.Radar;

namespace Confere.Radar.UnitTests.Validators
{
    public class StatusRequestValidatorTests
    {
        [Theory]
        [InlineData("DESCARTADO", true)]
        [InlineData("em_analise", true)]
        [InlineData("CONFIRMADO", false)]
        [InlineData("", false)]
        public void RN13_GestorSoDescartaOuReabre(string status, bool valido)
        {
            var resultado = new AtualizarStatusIdentificadorRequestValidator().Validate(new AtualizarStatusIdentificadorRequest(status));

            Assert.Equal(valido, resultado.IsValid);
        }

        [Theory]
        [InlineData("SOLICITADO", true)]
        [InlineData("concedido", true)]
        [InlineData("PROVAVEL", false)]
        [InlineData("INELEGIVEL", false)]
        [InlineData("", false)]
        public void RN16_CidadaoSoMarcaSolicitadoOuConcedido(string status, bool valido)
        {
            var resultado = new AtualizarElegibilidadeRequestValidator().Validate(new AtualizarElegibilidadeRequest(status));

            Assert.Equal(valido, resultado.IsValid);
        }

        [Fact]
        public void AtualizarDenuncia_SemTipoDeGolpe_DaErro()
        {
            var resultado = new AtualizarDenunciaRequestValidator().Validate(new AtualizarDenunciaRequest("", null));

            Assert.False(resultado.IsValid);
        }
    }
}
