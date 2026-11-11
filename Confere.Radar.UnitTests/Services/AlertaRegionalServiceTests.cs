using Confere.Radar.Application.Services;
using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Dtos.AlertasRegionais;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.UnitTests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Confere.Radar.UnitTests.Services
{
    public class AlertaRegionalServiceTests
    {
        private const int CodIbgeSaoPaulo = 3550308;

        private readonly Mock<IAlertaRegionalRepository> _alertaRepository = new();
        private readonly Mock<IDenunciaRepository> _denunciaRepository = new();
        private readonly Mock<IMunicipioRepository> _municipioRepository = new();
        private readonly Mock<ITipoGolpeRepository> _tipoGolpeRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly RelogioFixo _relogio = new();
        private readonly AlertaRegionalService _service;

        private readonly Municipio _saoPaulo = new() { IdMunicipio = 1, Nome = "São Paulo", Uf = "SP", CodIbge = CodIbgeSaoPaulo };
        private readonly TipoGolpe _falsaCentral = new() { IdTipoGolpe = 3, Codigo = "FALSA_CENTRAL", Nome = "Falsa central do banco", Gravidade = 5 };

        public AlertaRegionalServiceTests()
        {
            _municipioRepository.Setup(r => r.ObterPorCodIbgeAsync(CodIbgeSaoPaulo)).ReturnsAsync(_saoPaulo);
            _tipoGolpeRepository.Setup(r => r.ObterPorCodigoAsync("FALSA_CENTRAL")).ReturnsAsync(_falsaCentral);

            _service = new AlertaRegionalService(
                _alertaRepository.Object,
                _denunciaRepository.Object,
                _municipioRepository.Object,
                _tipoGolpeRepository.Object,
                _unitOfWork.Object,
                _relogio,
                NullLogger<AlertaRegionalService>.Instance);
        }

        [Fact]
        public async Task RN14_TresIdentificadoresConfirmadosSemAlertaAtivo_CriaAlertaAutomatico()
        {
            AlertaRegional? criado = null;
            _denunciaRepository.Setup(r => r.ContarIdentificadoresConfirmadosAsync(1, 3, _relogio.AgoraUtc.AddHours(-48))).ReturnsAsync(3);
            _alertaRepository.Setup(r => r.ExisteAtivoAsync(1, 3, _relogio.AgoraUtc)).ReturnsAsync(false);
            _alertaRepository.Setup(r => r.AdicionarAsync(It.IsAny<AlertaRegional>())).Callback<AlertaRegional>(a => criado = a);

            var resultado = await _service.AvaliarAlertaAutomaticoAsync(_saoPaulo, _falsaCentral);

            Assert.True(resultado);
            Assert.NotNull(criado);
            Assert.Equal(OrigemAlerta.Automatico, criado!.Origem);
            Assert.Equal(NivelAlerta.Atencao, criado.Nivel);
            Assert.Equal(TimeSpan.FromDays(7), criado.DtFim - criado.DtInicio);
            Assert.Null(criado.IdGestor);
            _unitOfWork.Verify(u => u.SalvarAsync(), Times.Once);
        }

        [Fact]
        public async Task RN14_MenosDeTresConfirmados_NaoCriaAlerta()
        {
            _denunciaRepository.Setup(r => r.ContarIdentificadoresConfirmadosAsync(1, 3, It.IsAny<DateTime>())).ReturnsAsync(2);

            var resultado = await _service.AvaliarAlertaAutomaticoAsync(_saoPaulo, _falsaCentral);

            Assert.False(resultado);
            _alertaRepository.Verify(r => r.AdicionarAsync(It.IsAny<AlertaRegional>()), Times.Never);
        }

        [Fact]
        public async Task RN14_JaExisteAlertaAtivoIgual_NaoCriaOutro()
        {
            _denunciaRepository.Setup(r => r.ContarIdentificadoresConfirmadosAsync(1, 3, It.IsAny<DateTime>())).ReturnsAsync(5);
            _alertaRepository.Setup(r => r.ExisteAtivoAsync(1, 3, It.IsAny<DateTime>())).ReturnsAsync(true);

            var resultado = await _service.AvaliarAlertaAutomaticoAsync(_saoPaulo, _falsaCentral);

            Assert.False(resultado);
            _alertaRepository.Verify(r => r.AdicionarAsync(It.IsAny<AlertaRegional>()), Times.Never);
        }

        [Fact]
        public async Task Criar_GestorDoMunicipio_CriaComOrigemGestor()
        {
            var request = NovoRequest(CodIbgeSaoPaulo, _relogio.GetUtcNow().AddDays(5));

            var resposta = await _service.CriarAsync(request, idGestor: 9, codIbgeGestor: CodIbgeSaoPaulo);

            Assert.Equal(OrigemAlerta.Gestor, resposta.Origem);
            Assert.Equal("URGENTE", resposta.Nivel);
            Assert.Equal(TimeSpan.FromHours(-3), resposta.DtInicio.Offset);
            _alertaRepository.Verify(r => r.AdicionarAsync(It.Is<AlertaRegional>(a => a.IdGestor == 9 && a.Ativo)), Times.Once);
        }

        [Fact]
        public async Task Criar_GestorDeOutroMunicipio_LancaSemPermissao()
        {
            var request = NovoRequest(CodIbgeSaoPaulo, _relogio.GetUtcNow().AddDays(5));

            await Assert.ThrowsAsync<SemPermissaoException>(() => _service.CriarAsync(request, 9, 3509502));
        }

        [Fact]
        public async Task Criar_GestorSemMunicipioNoToken_LancaSemPermissao()
        {
            var request = NovoRequest(CodIbgeSaoPaulo, _relogio.GetUtcNow().AddDays(5));

            await Assert.ThrowsAsync<SemPermissaoException>(() => _service.CriarAsync(request, 9, null));
        }

        [Fact]
        public async Task Criar_DataFimNoPassado_LancaValidacao()
        {
            var request = NovoRequest(CodIbgeSaoPaulo, _relogio.GetUtcNow().AddDays(-1)) with { DtInicio = _relogio.GetUtcNow().AddDays(-2) };

            var erro = await Assert.ThrowsAsync<ValidacaoException>(() => _service.CriarAsync(request, 9, CodIbgeSaoPaulo));

            Assert.Equal("dtFim", erro.Erros[0].Campo);
        }

        [Fact]
        public async Task Criar_TipoDeGolpeInexistente_LancaValidacao()
        {
            var request = NovoRequest(CodIbgeSaoPaulo, _relogio.GetUtcNow().AddDays(5)) with { TipoGolpeCodigo = "NAO_EXISTE" };

            await Assert.ThrowsAsync<ValidacaoException>(() => _service.CriarAsync(request, 9, CodIbgeSaoPaulo));
        }

        [Fact]
        public async Task Atualizar_AlertaEncerrado_LancaConflito()
        {
            _alertaRepository.Setup(r => r.ObterPorIdAsync(4)).ReturnsAsync(NovoAlerta(ativo: false));
            var request = new AtualizarAlertaRegionalRequest("FALSA_CENTRAL", "Título", "Mensagem", "INFO", _relogio.GetUtcNow().AddDays(3));

            await Assert.ThrowsAsync<ConflitoException>(() => _service.AtualizarAsync(4, request, 9, CodIbgeSaoPaulo));
        }

        [Fact]
        public async Task Atualizar_AlertaAtivo_TrocaOsDados()
        {
            var alerta = NovoAlerta(ativo: true);
            _alertaRepository.Setup(r => r.ObterPorIdAsync(4)).ReturnsAsync(alerta);
            var request = new AtualizarAlertaRegionalRequest("FALSA_CENTRAL", "Novo título", "Nova mensagem", "info", _relogio.GetUtcNow().AddDays(3));

            var resposta = await _service.AtualizarAsync(4, request, 9, CodIbgeSaoPaulo);

            Assert.Equal("Novo título", resposta.Titulo);
            Assert.Equal("INFO", resposta.Nivel);
            Assert.Equal(9, alerta.IdGestor);
        }

        [Fact]
        public async Task Encerrar_DesativaSemApagar()
        {
            var alerta = NovoAlerta(ativo: true);
            _alertaRepository.Setup(r => r.ObterPorIdAsync(4)).ReturnsAsync(alerta);

            await _service.EncerrarAsync(4, CodIbgeSaoPaulo);

            Assert.False(alerta.Ativo);
            _unitOfWork.Verify(u => u.SalvarAsync(), Times.Once);
        }

        [Fact]
        public async Task ObterPorId_Inexistente_LancaNaoEncontrado()
        {
            _alertaRepository.Setup(r => r.ObterPorIdAsync(99)).ReturnsAsync((AlertaRegional?)null);

            await Assert.ThrowsAsync<NaoEncontradoException>(() => _service.ObterPorIdAsync(99));
        }

        [Fact]
        public async Task ListarAtivos_MunicipioInexistente_LancaNaoEncontrado()
        {
            _municipioRepository.Setup(r => r.ObterPorCodIbgeAsync(1234567)).ReturnsAsync((Municipio?)null);

            await Assert.ThrowsAsync<NaoEncontradoException>(() => _service.ListarAtivosAsync(1234567));
        }

        private AlertaRegionalRequest NovoRequest(int codIbge, DateTimeOffset fim)
        {
            return new AlertaRegionalRequest(codIbge, "falsa_central", "Golpe da falsa central", "O banco não liga pedindo senha.", "urgente", null, fim);
        }

        private AlertaRegional NovoAlerta(bool ativo)
        {
            return new AlertaRegional
            {
                IdAlertaRegional = 4,
                IdMunicipio = 1,
                Municipio = _saoPaulo,
                IdTipoGolpe = 3,
                TipoGolpe = _falsaCentral,
                Titulo = "Alerta",
                Mensagem = "Mensagem",
                Nivel = NivelAlerta.Atencao,
                Origem = OrigemAlerta.Automatico,
                DtInicio = _relogio.AgoraUtc.AddDays(-1),
                DtFim = _relogio.AgoraUtc.AddDays(6),
                Ativo = ativo
            };
        }
    }
}
