using Confere.Radar.Application.Services;
using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Dtos.Radar;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.UnitTests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Confere.Radar.UnitTests.Services
{
    public class DenunciaServiceTests
    {
        private const int CodIbgeSaoPaulo = 3550308;

        private readonly Mock<IDenunciaRepository> _denunciaRepository = new();
        private readonly Mock<IIdentificadorRepository> _identificadorRepository = new();
        private readonly Mock<IMunicipioRepository> _municipioRepository = new();
        private readonly Mock<ITipoGolpeRepository> _tipoGolpeRepository = new();
        private readonly Mock<IRadarScoreService> _scoreService = new();
        private readonly Mock<IAlertaRegionalService> _alertaService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly RelogioFixo _relogio = new();
        private readonly DenunciaService _service;

        private readonly Municipio _saoPaulo = new() { IdMunicipio = 1, Nome = "São Paulo", Uf = "SP", CodIbge = CodIbgeSaoPaulo };
        private readonly TipoGolpe _falsaCentral = new() { IdTipoGolpe = 3, Codigo = "FALSA_CENTRAL", Nome = "Falsa central do banco" };
        private readonly TipoGolpe _falsoParente = new() { IdTipoGolpe = 4, Codigo = "FALSO_PARENTE", Nome = "Falso parente" };

        public DenunciaServiceTests()
        {
            _municipioRepository.Setup(r => r.ObterPorCodIbgeAsync(CodIbgeSaoPaulo)).ReturnsAsync(_saoPaulo);
            _tipoGolpeRepository.Setup(r => r.ObterPorCodigoAsync("FALSA_CENTRAL")).ReturnsAsync(_falsaCentral);
            _tipoGolpeRepository.Setup(r => r.ObterPorCodigoAsync("FALSO_PARENTE")).ReturnsAsync(_falsoParente);

            _service = new DenunciaService(
                _denunciaRepository.Object,
                _identificadorRepository.Object,
                _municipioRepository.Object,
                _tipoGolpeRepository.Object,
                new NormalizadorIdentificador(),
                _scoreService.Object,
                _alertaService.Object,
                _unitOfWork.Object,
                _relogio,
                NullLogger<DenunciaService>.Instance);
        }

        [Fact]
        public async Task Criar_IdentificadorNovo_GuardaSoHashEMascara()
        {
            IdentificadorSuspeito? novo = null;
            _identificadorRepository.Setup(r => r.ObterPorValorAsync("TELEFONE", It.IsAny<string>())).ReturnsAsync((IdentificadorSuspeito?)null);
            _identificadorRepository.Setup(r => r.AdicionarAsync(It.IsAny<IdentificadorSuspeito>())).Callback<IdentificadorSuspeito>(i => novo = i);

            var resposta = await _service.CriarAsync(NovoRequest(), idUsuario: 1);

            Assert.NotNull(novo);
            Assert.Equal(64, novo!.ValorHash.Length);
            Assert.DoesNotContain("912345678", novo.ValorHash);
            Assert.Equal("+55 11 9****-5678", resposta.Identificador.ValorMascarado);
            _denunciaRepository.Verify(r => r.ExisteDoUsuarioDesdeAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
            _scoreService.Verify(s => s.AtualizarIndicadoresAsync(novo, _relogio.AgoraUtc), Times.Once);
            _alertaService.Verify(a => a.AvaliarAlertaAutomaticoAsync(_saoPaulo, _falsaCentral), Times.Once);
        }

        [Fact]
        public async Task RN12_MesmoUsuarioMesmoIdentificadorEm24h_LancaDenunciaDuplicada()
        {
            var existente = new IdentificadorSuspeito { IdIdentificador = 10, Tipo = "TELEFONE" };
            _identificadorRepository.Setup(r => r.ObterPorValorAsync("TELEFONE", It.IsAny<string>())).ReturnsAsync(existente);
            _denunciaRepository.Setup(r => r.ExisteDoUsuarioDesdeAsync(1, 10, _relogio.AgoraUtc.AddHours(-24))).ReturnsAsync(true);

            var erro = await Assert.ThrowsAsync<ConflitoException>(() => _service.CriarAsync(NovoRequest(), 1));

            Assert.Equal(CodigoErro.DenunciaDuplicada, erro.Codigo);
            _denunciaRepository.Verify(r => r.AdicionarAsync(It.IsAny<Denuncia>()), Times.Never);
        }

        [Fact]
        public async Task RN12_IdentificadorExistenteSemDenunciaRecente_Aceita()
        {
            var existente = new IdentificadorSuspeito { IdIdentificador = 10, Tipo = "TELEFONE", ValorMascarado = "+55 11 9****-5678" };
            _identificadorRepository.Setup(r => r.ObterPorValorAsync("TELEFONE", It.IsAny<string>())).ReturnsAsync(existente);
            _denunciaRepository.Setup(r => r.ExisteDoUsuarioDesdeAsync(1, 10, It.IsAny<DateTime>())).ReturnsAsync(false);

            await _service.CriarAsync(NovoRequest(), 1);

            _denunciaRepository.Verify(r => r.AdicionarAsync(It.Is<Denuncia>(d => d.Identificador == existente && d.IdUsuario == 1)), Times.Once);
            _identificadorRepository.Verify(r => r.AdicionarAsync(It.IsAny<IdentificadorSuspeito>()), Times.Never);
        }

        [Fact]
        public async Task Criar_MunicipioInexistente_LancaValidacao()
        {
            var request = NovoRequest() with { CodIbgeMunicipio = 1234567 };
            _municipioRepository.Setup(r => r.ObterPorCodIbgeAsync(1234567)).ReturnsAsync((Municipio?)null);

            var erro = await Assert.ThrowsAsync<ValidacaoException>(() => _service.CriarAsync(request, 1));

            Assert.Equal("codIbgeMunicipio", erro.Erros[0].Campo);
        }

        [Fact]
        public async Task Criar_TipoDeGolpeInexistente_LancaValidacao()
        {
            var request = NovoRequest() with { TipoGolpeCodigo = "INVENTADO" };

            var erro = await Assert.ThrowsAsync<ValidacaoException>(() => _service.CriarAsync(request, 1));

            Assert.Equal("tipoGolpeCodigo", erro.Erros[0].Campo);
        }

        [Fact]
        public async Task Obter_DenunciaDeOutroUsuario_LancaSemPermissao()
        {
            _denunciaRepository.Setup(r => r.ObterPorIdAsync(5)).ReturnsAsync(NovaDenuncia(idUsuario: 2));

            await Assert.ThrowsAsync<SemPermissaoException>(() => _service.ObterAsync(5, idUsuario: 1));
        }

        [Fact]
        public async Task Obter_DenunciaInexistente_LancaNaoEncontrado()
        {
            _denunciaRepository.Setup(r => r.ObterPorIdAsync(5)).ReturnsAsync((Denuncia?)null);

            await Assert.ThrowsAsync<NaoEncontradoException>(() => _service.ObterAsync(5, 1));
        }

        [Fact]
        public async Task Atualizar_TrocandoTipo_ReavaliaAlertaDoNovoTipo()
        {
            var denuncia = NovaDenuncia(idUsuario: 1);
            _denunciaRepository.Setup(r => r.ObterPorIdAsync(5)).ReturnsAsync(denuncia);

            var resposta = await _service.AtualizarAsync(5, new AtualizarDenunciaRequest("FALSO_PARENTE", "  "), 1);

            Assert.Equal("FALSO_PARENTE", resposta.TipoGolpe.Codigo);
            Assert.Null(denuncia.Descricao);
            Assert.NotNull(denuncia.DtAtualizacao);
            _alertaService.Verify(a => a.AvaliarAlertaAutomaticoAsync(_saoPaulo, _falsoParente), Times.Once);
        }

        [Fact]
        public async Task Atualizar_MesmoTipo_NaoReavaliaAlerta()
        {
            _denunciaRepository.Setup(r => r.ObterPorIdAsync(5)).ReturnsAsync(NovaDenuncia(idUsuario: 1));

            await _service.AtualizarAsync(5, new AtualizarDenunciaRequest("FALSA_CENTRAL", "Outra descrição"), 1);

            _alertaService.Verify(a => a.AvaliarAlertaAutomaticoAsync(It.IsAny<Municipio>(), It.IsAny<TipoGolpe>()), Times.Never);
        }

        [Fact]
        public async Task Excluir_RemoveERecalculaOIdentificador()
        {
            var denuncia = NovaDenuncia(idUsuario: 1);
            _denunciaRepository.Setup(r => r.ObterPorIdAsync(5)).ReturnsAsync(denuncia);

            await _service.ExcluirAsync(5, 1);

            _denunciaRepository.Verify(r => r.Remover(denuncia), Times.Once);
            _scoreService.Verify(s => s.AtualizarIndicadoresAsync(denuncia.Identificador!, It.IsAny<DateTime>()), Times.Once);
            _unitOfWork.Verify(u => u.SalvarAsync(), Times.Exactly(2));
        }

        private static DenunciaRequest NovoRequest()
        {
            return new DenunciaRequest("TELEFONE", "+55 11 91234-5678", "FALSA_CENTRAL", CodIbgeSaoPaulo, "Ligaram dizendo ser do banco", null);
        }

        private Denuncia NovaDenuncia(int idUsuario)
        {
            return new Denuncia
            {
                IdDenuncia = 5,
                IdUsuario = idUsuario,
                IdIdentificador = 10,
                Identificador = new IdentificadorSuspeito { IdIdentificador = 10, Tipo = "TELEFONE", ValorMascarado = "+55 11 9****-5678" },
                IdTipoGolpe = 3,
                TipoGolpe = _falsaCentral,
                IdMunicipio = 1,
                Municipio = _saoPaulo,
                Descricao = "Descrição",
                DtDenuncia = _relogio.AgoraUtc.AddHours(-1)
            };
        }
    }
}
