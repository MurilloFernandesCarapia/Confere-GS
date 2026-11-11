using Confere.Radar.Application.Validators;
using Confere.Radar.Domain.Dtos.Direitos;

namespace Confere.Radar.UnitTests.Validators
{
    public class PerfilSocialRequestValidatorTests
    {
        private readonly PerfilSocialRequestValidator _validator = new();

        [Fact]
        public void PerfilDoContrato_EhValido()
        {
            var request = new PerfilSocialRequest(68, "ATE_1_4", 2, true, false, false, false, 3550308, true);

            Assert.True(_validator.Validate(request).IsValid);
        }

        [Fact]
        public void RN17_PerfilSoComConsentimento_EhValido()
        {
            var request = new PerfilSocialRequest(null, null, null, null, null, null, null, null, true);

            Assert.True(_validator.Validate(request).IsValid);
        }

        [Theory]
        [InlineData("1500")]
        [InlineData("1500,00")]
        [InlineData("ATE_3")]
        public void RN17_RendaSoEmFaixa_ValorExatoDaErro(string renda)
        {
            var request = new PerfilSocialRequest(null, renda, null, null, null, null, null, null, true);

            Assert.Contains(_validator.Validate(request).Errors, e => e.PropertyName == "RendaPcFaixa");
        }

        [Fact]
        public void IdadeEMoradoresForaDoLimite_DaErro()
        {
            var request = new PerfilSocialRequest(131, null, 0, null, null, null, null, 123, true);
            var erros = _validator.Validate(request).Errors;

            Assert.Contains(erros, e => e.PropertyName == "Idade");
            Assert.Contains(erros, e => e.PropertyName == "QtdMoradores");
            Assert.Contains(erros, e => e.PropertyName == "CodIbgeMunicipio");
        }
    }
}
