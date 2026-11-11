using Confere.Radar.Application.Services;
using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Exceptions;

namespace Confere.Radar.UnitTests.Services
{
    public class NormalizadorIdentificadorTests
    {
        private readonly NormalizadorIdentificador _normalizador = new();

        [Theory]
        [InlineData("+55 (11) 91234-5678")]
        [InlineData("11912345678")]
        [InlineData("(11) 91234-5678")]
        [InlineData("5511912345678")]
        public void RN11_Telefone_EmFormatosDiferentes_GeraOMesmoValorEHash(string valor)
        {
            var resultado = _normalizador.Normalizar("TELEFONE", valor);

            Assert.Equal("5511912345678", resultado.ValorNormalizado);
            Assert.Equal(_normalizador.Normalizar("TELEFONE", "11912345678").Hash, resultado.Hash);
            Assert.Equal("+55 11 9****-5678", resultado.ValorMascarado);
        }

        [Fact]
        public void RN11_TelefoneFixo_MascaraSemONove()
        {
            var resultado = _normalizador.Normalizar("TELEFONE", "11 3333-4444");

            Assert.Equal("551133334444", resultado.ValorNormalizado);
            Assert.Equal("+55 11 ****-4444", resultado.ValorMascarado);
        }

        [Theory]
        [InlineData("123")]
        [InlineData("abc")]
        [InlineData("4411912345678")]
        [InlineData("119123456789012")]
        public void RN11_TelefoneInvalido_LancaValidacao(string valor)
        {
            var erro = Assert.Throws<ValidacaoException>(() => _normalizador.Normalizar("TELEFONE", valor));

            Assert.Equal("valor", erro.Erros[0].Campo);
        }

        [Fact]
        public void RN11_Link_RemoveProtocoloWwwUtmEFragmento()
        {
            var completo = _normalizador.Normalizar("LINK", "https://www.Golpe-Gov.com/pagamento/?utm_source=zap&id=2#topo");
            var simples = _normalizador.Normalizar("LINK", "golpe-gov.com/pagamento?id=2");

            Assert.Equal("golpe-gov.com/pagamento?id=2", completo.ValorNormalizado);
            Assert.Equal(simples.Hash, completo.Hash);
            Assert.Equal("golpe-gov.com/…", completo.ValorMascarado);
        }

        [Fact]
        public void RN11_LinkSoComUtm_FicaSemParametros()
        {
            var resultado = _normalizador.Normalizar("LINK", "http://site-falso.com/?utm_campaign=x&utm_medium=y");

            Assert.Equal("site-falso.com", resultado.ValorNormalizado);
            Assert.Equal("site-falso.com", resultado.ValorMascarado);
        }

        [Theory]
        [InlineData("semponto")]
        [InlineData("link com espaco.com")]
        [InlineData("https://.com")]
        public void RN11_LinkInvalido_LancaValidacao(string valor)
        {
            Assert.Throws<ValidacaoException>(() => _normalizador.Normalizar("LINK", valor));
        }

        [Fact]
        public void RN11_Email_FicaMinusculoSemEspacosEMascarado()
        {
            var resultado = _normalizador.Normalizar("EMAIL", " Fraude@Banco-Falso.com ");

            Assert.Equal("fraude@banco-falso.com", resultado.ValorNormalizado);
            Assert.Equal("fr***@banco-falso.com", resultado.ValorMascarado);
        }

        [Fact]
        public void RN11_EmailInvalido_LancaValidacao()
        {
            Assert.Throws<ValidacaoException>(() => _normalizador.Normalizar("EMAIL", "nao-e-email"));
        }

        [Fact]
        public void RN11_ChavePix_FicaMinusculaSemEspacos()
        {
            var resultado = _normalizador.Normalizar("CHAVE_PIX", "ABC 123 DEF 456");

            Assert.Equal("abc123def456", resultado.ValorNormalizado);
            Assert.Equal("abc****56", resultado.ValorMascarado);
        }

        [Fact]
        public void RN11_ChavePixCurta_LancaValidacao()
        {
            Assert.Throws<ValidacaoException>(() => _normalizador.Normalizar("CHAVE_PIX", "ab"));
        }

        [Fact]
        public void RN11_Perfil_RemoveArrobaEMascara()
        {
            var resultado = _normalizador.Normalizar("PERFIL", "@Suporte_Banco_Oficial");

            Assert.Equal("suporte_banco_oficial", resultado.ValorNormalizado);
            Assert.StartsWith("@sup", resultado.ValorMascarado);
        }

        [Fact]
        public void RN11_TipoEmMinusculo_EhAceito()
        {
            var resultado = _normalizador.Normalizar("telefone", "11912345678");

            Assert.Equal(TipoIdentificador.Telefone, resultado.Tipo);
        }

        [Fact]
        public void RN11_Hash_EhSha256EmHexMinusculo()
        {
            var resultado = _normalizador.Normalizar("EMAIL", "golpe@site.com");

            Assert.Equal(64, resultado.Hash.Length);
            Assert.Matches("^[0-9a-f]{64}$", resultado.Hash);
        }

        [Fact]
        public void RN11_TipoInvalido_LancaValidacaoNoCampoTipo()
        {
            var erro = Assert.Throws<ValidacaoException>(() => _normalizador.Normalizar("FAX", "123"));

            Assert.Equal("tipo", erro.Erros[0].Campo);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void RN11_ValorVazio_LancaValidacao(string valor)
        {
            Assert.Throws<ValidacaoException>(() => _normalizador.Normalizar("TELEFONE", valor));
        }
    }
}
