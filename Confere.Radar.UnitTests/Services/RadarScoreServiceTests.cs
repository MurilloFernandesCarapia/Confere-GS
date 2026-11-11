using Confere.Radar.Application.Services;
using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Domain.Models;
using Moq;

namespace Confere.Radar.UnitTests.Services
{
    public class RadarScoreServiceTests
    {
        private static readonly DateTime Agora = new(2026, 11, 18, 17, 0, 0, DateTimeKind.Utc);

        private readonly Mock<IDenunciaRepository> _denunciaRepository = new();
        private readonly RadarScoreService _service;

        public RadarScoreServiceTests()
        {
            _service = new RadarScoreService(_denunciaRepository.Object);
        }

        [Theory]
        [InlineData(0, 1, 1, 0)]
        [InlineData(1, 1, 1, 32)]
        [InlineData(4, 1, 1, 68)]
        [InlineData(5, 1, 1, 80)]
        [InlineData(5, 1, 72, 60)]
        [InlineData(2, 2, 1, 54)]
        [InlineData(5, 3, 1, 100)]
        [InlineData(10, 1, 1, 100)]
        public void RN13_CalcularScore_SegueAFormula(int total, int municipios, int horasDesdeUltima, int esperado)
        {
            var estatistica = new EstatisticaIdentificador(total, municipios, Agora.AddDays(-5), Agora.AddHours(-horasDesdeUltima));

            Assert.Equal(esperado, _service.CalcularScore(estatistica, Agora));
        }

        [Fact]
        public void RN13_DenunciaCom48HorasExatas_NaoGanhaBonus()
        {
            var estatistica = new EstatisticaIdentificador(1, 1, Agora.AddHours(-48), Agora.AddHours(-48));

            Assert.Equal(12, _service.CalcularScore(estatistica, Agora));
        }

        [Theory]
        [InlineData(StatusIdentificador.EmAnalise, 5, 70, StatusIdentificador.Confirmado)]
        [InlineData(StatusIdentificador.EmAnalise, 5, 69, StatusIdentificador.EmAnalise)]
        [InlineData(StatusIdentificador.EmAnalise, 4, 100, StatusIdentificador.EmAnalise)]
        [InlineData(StatusIdentificador.Confirmado, 5, 60, StatusIdentificador.Confirmado)]
        [InlineData(StatusIdentificador.Confirmado, 4, 80, StatusIdentificador.EmAnalise)]
        [InlineData(StatusIdentificador.Descartado, 10, 100, StatusIdentificador.Descartado)]
        public void RN13_DefinirStatus(string statusAtual, int total, int score, string esperado)
        {
            Assert.Equal(esperado, _service.DefinirStatus(statusAtual, total, score));
        }

        [Fact]
        public async Task RN13_AtualizarIndicadores_PromoveParaConfirmado()
        {
            var identificador = new IdentificadorSuspeito { IdIdentificador = 7, Status = StatusIdentificador.EmAnalise };
            _denunciaRepository.Setup(r => r.ObterEstatisticaAsync(7))
                .ReturnsAsync(new EstatisticaIdentificador(5, 1, Agora.AddHours(-10), Agora.AddMinutes(-5)));

            await _service.AtualizarIndicadoresAsync(identificador, Agora);

            Assert.Equal(5, identificador.TotalDenuncias);
            Assert.Equal(80, identificador.Score);
            Assert.Equal(StatusIdentificador.Confirmado, identificador.Status);
            Assert.Equal(Agora.AddHours(-10), identificador.DtPrimeira);
            Assert.Equal(Agora.AddMinutes(-5), identificador.DtUltima);
        }

        [Fact]
        public async Task RN13_AtualizarIndicadores_SemDenuncias_ZeraSemMexerNasDatas()
        {
            var primeira = Agora.AddDays(-3);
            var identificador = new IdentificadorSuspeito { IdIdentificador = 8, Status = StatusIdentificador.EmAnalise, TotalDenuncias = 1, Score = 32, DtPrimeira = primeira, DtUltima = primeira };
            _denunciaRepository.Setup(r => r.ObterEstatisticaAsync(8)).ReturnsAsync(new EstatisticaIdentificador(0, 0, null, null));

            await _service.AtualizarIndicadoresAsync(identificador, Agora);

            Assert.Equal(0, identificador.TotalDenuncias);
            Assert.Equal(0, identificador.Score);
            Assert.Equal(primeira, identificador.DtPrimeira);
        }
    }
}
