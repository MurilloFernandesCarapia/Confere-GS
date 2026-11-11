using Confere.Radar.Application.Services;
using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Dtos.Gestao;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Domain.Models;
using Confere.Radar.Domain.Pagination;
using Confere.Radar.UnitTests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Confere.Radar.UnitTests.Services
{
    public class GestaoServiceTests
    {
        private const int CodIbgeSaoPaulo = 3550308;

        private readonly Mock<IIdentificadorRepository> _identificadorRepository = new();
        private readonly Mock<IDenunciaRepository> _denunciaRepository = new();
        private readonly Mock<IAlertaRegionalRepository> _alertaRepository = new();
        private readonly Mock<IMunicipioRepository> _municipioRepository = new();
        private readonly Mock<IRadarScoreService> _scoreService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly RelogioFixo _relogio = new();
        private readonly GestaoService _service;

        public GestaoServiceTests()
        {
            _municipioRepository.Setup(r => r.ObterPorCodIbgeAsync(CodIbgeSaoPaulo))
                .ReturnsAsync(new Municipio { IdMunicipio = 1, Nome = "São Paulo", Uf = "SP", CodIbge = CodIbgeSaoPaulo });
            _alertaRepository.Setup(r => r.ListarAtivosAsync(1, It.IsAny<DateTime>())).ReturnsAsync(new List<AlertaRegional>());
            _denunciaRepository.Setup(r => r.ListarTiposMaisDenunciadosAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), 5))
                .ReturnsAsync(new List<TotalPorTipoGolpe> { new("FALSA_CENTRAL", "Falsa central do banco", 7) });

            _service = new GestaoService(
                _identificadorRepository.Object,
                _denunciaRepository.Object,
                _alertaRepository.Object,
                _municipioRepository.Object,
                _scoreService.Object,
                _unitOfWork.Object,
                _relogio,
                NullLogger<GestaoService>.Instance);
        }

        [Fact]
        public async Task Painel_SemParametros_UsaMunicipioDoGestorEUltimos30Dias()
        {
            _denunciaRepository.Setup(r => r.ContarNoPeriodoAsync(1, _relogio.AgoraUtc.AddDays(-30), _relogio.AgoraUtc)).ReturnsAsync(12);

            var painel = await _service.ObterPainelAsync(null, null, null, CodIbgeSaoPaulo);

            Assert.Equal(CodIbgeSaoPaulo, painel.Municipio.CodIbge);
            Assert.Equal(12, painel.TotalDenuncias);
            Assert.Equal(TimeSpan.FromDays(30), painel.Ate - painel.De);
            Assert.Single(painel.TiposMaisDenunciados);
        }

        [Fact]
        public async Task Painel_DeOutroMunicipio_LancaSemPermissao()
        {
            await Assert.ThrowsAsync<SemPermissaoException>(() => _service.ObterPainelAsync(3509502, null, null, CodIbgeSaoPaulo));
        }

        [Fact]
        public async Task Painel_DataInicialDepoisDaFinal_LancaValidacao()
        {
            var ate = _relogio.GetUtcNow().AddDays(-10);

            await Assert.ThrowsAsync<ValidacaoException>(() => _service.ObterPainelAsync(null, ate.AddDays(1), ate, CodIbgeSaoPaulo));
        }

        [Fact]
        public async Task Painel_PeriodoMaiorQueUmAno_LancaValidacao()
        {
            var ate = _relogio.GetUtcNow();

            await Assert.ThrowsAsync<ValidacaoException>(() => _service.ObterPainelAsync(null, ate.AddDays(-400), ate, CodIbgeSaoPaulo));
        }

        [Fact]
        public async Task ListarIdentificadores_StatusInvalido_LancaValidacao()
        {
            await Assert.ThrowsAsync<ValidacaoException>(() => _service.ListarIdentificadoresAsync("QUALQUER", new ParametrosPaginacao()));
        }

        [Fact]
        public async Task ListarIdentificadores_StatusEmMinusculo_FiltraPeloStatus()
        {
            _identificadorRepository.Setup(r => r.ListarAsync(StatusIdentificador.Confirmado, It.IsAny<ParametrosPaginacao>()))
                .ReturnsAsync(new PaginaResultado<IdentificadorSuspeito> { Conteudo = new List<IdentificadorSuspeito>(), Pagina = 0, Tamanho = 10 });

            var pagina = await _service.ListarIdentificadoresAsync("confirmado", new ParametrosPaginacao());

            Assert.Empty(pagina.Conteudo);
        }

        [Fact]
        public async Task AtualizarStatus_Descartado_NaoRecalcula()
        {
            var identificador = new IdentificadorSuspeito { IdIdentificador = 3, Status = StatusIdentificador.Confirmado, TotalDenuncias = 6, Score = 92, DtPrimeira = _relogio.AgoraUtc.AddDays(-2), DtUltima = _relogio.AgoraUtc };
            _identificadorRepository.Setup(r => r.ObterPorIdAsync(3)).ReturnsAsync(identificador);

            var resposta = await _service.AtualizarStatusIdentificadorAsync(3, new AtualizarStatusIdentificadorRequest("DESCARTADO"));

            Assert.Equal(StatusIdentificador.Descartado, resposta.Status);
            _scoreService.Verify(s => s.AtualizarIndicadoresAsync(It.IsAny<IdentificadorSuspeito>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarStatus_EmAnalise_ReabreERecalcula()
        {
            var identificador = new IdentificadorSuspeito { IdIdentificador = 3, Status = StatusIdentificador.Descartado, DtPrimeira = _relogio.AgoraUtc.AddDays(-2), DtUltima = _relogio.AgoraUtc };
            _identificadorRepository.Setup(r => r.ObterPorIdAsync(3)).ReturnsAsync(identificador);

            await _service.AtualizarStatusIdentificadorAsync(3, new AtualizarStatusIdentificadorRequest("EM_ANALISE"));

            _scoreService.Verify(s => s.AtualizarIndicadoresAsync(It.Is<IdentificadorSuspeito>(i => i.Status == StatusIdentificador.EmAnalise), _relogio.AgoraUtc), Times.Once);
        }

        [Fact]
        public async Task AtualizarStatus_Inexistente_LancaNaoEncontrado()
        {
            _identificadorRepository.Setup(r => r.ObterPorIdAsync(99)).ReturnsAsync((IdentificadorSuspeito?)null);

            await Assert.ThrowsAsync<NaoEncontradoException>(() => _service.AtualizarStatusIdentificadorAsync(99, new AtualizarStatusIdentificadorRequest("DESCARTADO")));
        }
    }
}
