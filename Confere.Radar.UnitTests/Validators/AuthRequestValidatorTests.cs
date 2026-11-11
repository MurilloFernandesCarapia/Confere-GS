using Confere.Radar.Application.Validators;
using Confere.Radar.Domain.Dtos.Auth;

namespace Confere.Radar.UnitTests.Validators
{
    public class AuthRequestValidatorTests
    {
        private readonly RegistroCidadaoRequestValidator _registro = new();
        private readonly LoginRequestValidator _login = new();

        [Fact]
        public void Registro_Valido_NaoTemErros()
        {
            var request = new RegistroCidadaoRequest("Maria", "maria@teste.com", "Senha123", "11912345678", 3550308);

            Assert.True(_registro.Validate(request).IsValid);
        }

        [Theory]
        [InlineData("curta1")]
        [InlineData("semnumero")]
        [InlineData("12345678")]
        public void Registro_SenhaFraca_DaErro(string senha)
        {
            var request = new RegistroCidadaoRequest("Maria", "maria@teste.com", senha, null, 3550308);

            Assert.Contains(_registro.Validate(request).Errors, e => e.PropertyName == "Senha");
        }

        [Fact]
        public void Registro_EmailECodigoIbgeInvalidos_DaErro()
        {
            var erros = _registro.Validate(new RegistroCidadaoRequest("Maria", "maria", "Senha123", null, 35)).Errors;

            Assert.Contains(erros, e => e.PropertyName == "Email");
            Assert.Contains(erros, e => e.PropertyName == "CodIbgeMunicipio");
        }

        [Fact]
        public void Login_Vazio_DaErroNosDoisCampos()
        {
            var erros = _login.Validate(new LoginRequest("", "")).Errors;

            Assert.Contains(erros, e => e.PropertyName == "Email");
            Assert.Contains(erros, e => e.PropertyName == "Senha");
        }
    }
}
