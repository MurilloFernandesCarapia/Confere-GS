using Confere.Radar.Application.Services;
using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;
using Moq;

namespace Confere.Radar.UnitTests.Services
{
    public class RadarServiceTests
    {
        private readonly Mock<IIdentificadorRepository> _identificadorRepository = new();
        private readonly Mock<IDenunciaRepository> _denunciaRepository = new();
        private readonly Mock<ITipoGolpeRepository> _tipoGolpeRepository = new();
        private readonly NormalizadorIdentificador _normalizador = new();
        private readonly RadarService _service;

        public RadarServiceTests()
        {
            _service = new RadarService(_identificadorRepository.Object, _denunciaRepository.Object, _tipoGolpeRepository.Object, _normalizador);
        }

        [Fact]
        public async Task RN15_IdentificadorNaoDenunciado_DevolveNaoEncontrado()
        {
            _identificadorRepository.Setup(r => r.ObterPorValorAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((IdentificadorSuspeito?)null);

            var resposta = await _service.ConsultarAsync("TELEFONE", "11912345678");

            Assert.False(resposta.Encontrado);
            Assert.Equal(0, resposta.TotalDenuncias);
            Assert.Null(resposta.Status);
        }

        [Fact]
        public async Task RN15_IdentificadorDenunciado_DevolveSoStatusTotalTipoEData()
        {
            var hash = _normalizador.Normalizar("TELEFONE", "11912345678").Hash;
            var ultima = new DateTime(2026, 11, 18, 17, 32, 0, DateTimeKind.Utc);
            _identificadorRepository.Setup(r => r.ObterPorValorAsync("TELEFONE", hash)).ReturnsAsync(new IdentificadorSuspeito
            {
                IdIdentificador = 55,
                Status = StatusIdentificador.Confirmado,
                TotalDenuncias = 7,
                DtUltima = ultima
            });
            _denunciaRepository.Setup(r => r.ObterTipoMaisComumAsync(55)).ReturnsAsync(new TipoGolpe { Nome = "Falsa central do banco" });

            var resposta = await _service.ConsultarAsync("TELEFONE", "+55 (11) 91234-5678");

            Assert.True(resposta.Encontrado);
            Assert.Equal(StatusIdentificador.Confirmado, resposta.Status);
            Assert.Equal(7, resposta.TotalDenuncias);
            Assert.Equal("Falsa central do banco", resposta.TipoMaisComum);
            Assert.Equal(new DateTimeOffset(2026, 11, 18, 14, 32, 0, TimeSpan.FromHours(-3)), resposta.UltimaDenuncia);
        }

        [Fact]
        public async Task RN15_IdentificadorComZeroDenuncias_ContaComoNaoEncontrado()
        {
            _identificadorRepository.Setup(r => r.ObterPorValorAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new IdentificadorSuspeito { IdIdentificador = 1, TotalDenuncias = 0 });

            var resposta = await _service.ConsultarAsync("TELEFONE", "11912345678");

            Assert.False(resposta.Encontrado);
        }

        [Theory]
        [InlineData(null, "11912345678")]
        [InlineData("", "11912345678")]
        [InlineData("TELEFONE", null)]
        public async Task Consulta_SemTipoOuValor_LancaValidacao(string? tipo, string? valor)
        {
            await Assert.ThrowsAsync<ValidacaoException>(() => _service.ConsultarAsync(tipo, valor));
        }

        [Fact]
        public async Task ListarTiposGolpe_ConverteParaResposta()
        {
            _tipoGolpeRepository.Setup(r => r.ListarAsync()).ReturnsAsync(new List<TipoGolpe>
            {
                new() { Codigo = "FALSA_CENTRAL", Nome = "Falsa central do banco", Gravidade = 5, OrgaoImitado = "Bancos" }
            });

            var tipos = await _service.ListarTiposGolpeAsync();

            Assert.Equal("FALSA_CENTRAL", Assert.Single(tipos).Codigo);
        }
    }
}
