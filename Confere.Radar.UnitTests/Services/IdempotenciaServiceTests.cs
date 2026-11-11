using Confere.Radar.Application.Services;
using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.UnitTests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Confere.Radar.UnitTests.Services
{
    public class IdempotenciaServiceTests
    {
        private const string Chave = "6f1c2d0e-8a4b-4c1d-9e2f-3a5b7c9d1e2f";
        private const string Endpoint = "POST /denuncias";
        private const string Hash = "hash-do-corpo";

        private readonly Mock<IIdempotenciaRepository> _repository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly RelogioFixo _relogio = new();
        private readonly IdempotenciaService _service;

        public IdempotenciaServiceTests()
        {
            _service = new IdempotenciaService(_repository.Object, _unitOfWork.Object, _relogio, NullLogger<IdempotenciaService>.Instance);
        }

        [Fact]
        public async Task ChaveNova_GravaProcessandoComValidadeDe24h()
        {
            RegistroIdempotencia? gravado = null;
            _repository.Setup(r => r.ObterAsync(Chave)).ReturnsAsync((RegistroIdempotencia?)null);
            _repository.Setup(r => r.AdicionarAsync(It.IsAny<RegistroIdempotencia>())).Callback<RegistroIdempotencia>(r => gravado = r);

            var inicio = await _service.IniciarAsync(Chave, 1, Endpoint, Hash);

            Assert.False(inicio.Repetida);
            Assert.NotNull(gravado);
            Assert.Equal(SituacaoIdempotencia.Processando, gravado!.Situacao);
            Assert.Equal(TimeSpan.FromHours(24), gravado.DtExpiracao - gravado.DtCriacao);
            _unitOfWork.Verify(u => u.SalvarAsync(), Times.Once);
        }

        [Fact]
        public async Task MesmaChaveMesmoCorpoConcluida_DevolveRespostaSalva()
        {
            _repository.Setup(r => r.ObterAsync(Chave)).ReturnsAsync(Registro(SituacaoIdempotencia.Concluida));

            var inicio = await _service.IniciarAsync(Chave, 1, Endpoint, Hash);

            Assert.True(inicio.Repetida);
            Assert.Equal(201, inicio.StatusHttp);
            Assert.Equal("{\"id\":10}", inicio.CorpoResposta);
            _unitOfWork.Verify(u => u.SalvarAsync(), Times.Never);
        }

        [Fact]
        public async Task MesmaChaveCorpoDiferente_Lanca422()
        {
            _repository.Setup(r => r.ObterAsync(Chave)).ReturnsAsync(Registro(SituacaoIdempotencia.Concluida));

            await Assert.ThrowsAsync<IdempotenciaCorpoDiferenteException>(() => _service.IniciarAsync(Chave, 1, Endpoint, "outro-hash"));
        }

        [Fact]
        public async Task MesmaChaveOutroUsuario_Lanca422()
        {
            _repository.Setup(r => r.ObterAsync(Chave)).ReturnsAsync(Registro(SituacaoIdempotencia.Concluida));

            await Assert.ThrowsAsync<IdempotenciaCorpoDiferenteException>(() => _service.IniciarAsync(Chave, 2, Endpoint, Hash));
        }

        [Fact]
        public async Task MesmaChaveOutroEndpoint_Lanca422()
        {
            _repository.Setup(r => r.ObterAsync(Chave)).ReturnsAsync(Registro(SituacaoIdempotencia.Concluida));

            await Assert.ThrowsAsync<IdempotenciaCorpoDiferenteException>(() => _service.IniciarAsync(Chave, 1, "POST /elegibilidades/avaliar", Hash));
        }

        [Fact]
        public async Task MesmaChaveAindaProcessando_Lanca409()
        {
            _repository.Setup(r => r.ObterAsync(Chave)).ReturnsAsync(Registro(SituacaoIdempotencia.Processando));

            var erro = await Assert.ThrowsAsync<ConflitoException>(() => _service.IniciarAsync(Chave, 1, Endpoint, Hash));

            Assert.Equal(CodigoErro.IdempotenciaEmProcessamento, erro.Codigo);
        }

        [Fact]
        public async Task ChaveExpirada_EhReaproveitadaComoNova()
        {
            var registro = Registro(SituacaoIdempotencia.Concluida);
            registro.DtExpiracao = _relogio.AgoraUtc.AddMinutes(-1);
            _repository.Setup(r => r.ObterAsync(Chave)).ReturnsAsync(registro);

            var inicio = await _service.IniciarAsync(Chave, 2, Endpoint, "outro-hash");

            Assert.False(inicio.Repetida);
            Assert.Equal(SituacaoIdempotencia.Processando, registro.Situacao);
            Assert.Equal(2, registro.IdUsuario);
            Assert.Null(registro.CorpoResposta);
            _repository.Verify(r => r.AdicionarAsync(It.IsAny<RegistroIdempotencia>()), Times.Never);
        }

        [Fact]
        public async Task Concluir_SalvaStatusECorpo()
        {
            var registro = Registro(SituacaoIdempotencia.Processando);
            _repository.Setup(r => r.ObterAsync(Chave)).ReturnsAsync(registro);

            await _service.ConcluirAsync(Chave, 201, "{\"id\":11}");

            Assert.Equal(SituacaoIdempotencia.Concluida, registro.Situacao);
            Assert.Equal(201, registro.StatusHttp);
            Assert.Equal("{\"id\":11}", registro.CorpoResposta);
        }

        [Fact]
        public async Task Cancelar_DescartaAlteracoesELiberaAChave()
        {
            var registro = Registro(SituacaoIdempotencia.Processando);
            _repository.Setup(r => r.ObterAsync(Chave)).ReturnsAsync(registro);

            await _service.CancelarAsync(Chave);

            _unitOfWork.Verify(u => u.DescartarAlteracoes(), Times.Once);
            _repository.Verify(r => r.Remover(registro), Times.Once);
            _unitOfWork.Verify(u => u.SalvarAsync(), Times.Once);
        }

        [Fact]
        public async Task Cancelar_NaoApagaRegistroConcluido()
        {
            var registro = Registro(SituacaoIdempotencia.Concluida);
            _repository.Setup(r => r.ObterAsync(Chave)).ReturnsAsync(registro);

            await _service.CancelarAsync(Chave);

            _repository.Verify(r => r.Remover(It.IsAny<RegistroIdempotencia>()), Times.Never);
        }

        private RegistroIdempotencia Registro(string situacao)
        {
            return new RegistroIdempotencia
            {
                Chave = Chave,
                IdUsuario = 1,
                Endpoint = Endpoint,
                HashRequisicao = Hash,
                Situacao = situacao,
                StatusHttp = situacao == SituacaoIdempotencia.Concluida ? 201 : null,
                CorpoResposta = situacao == SituacaoIdempotencia.Concluida ? "{\"id\":10}" : null,
                DtCriacao = _relogio.AgoraUtc.AddHours(-1),
                DtExpiracao = _relogio.AgoraUtc.AddHours(23)
            };
        }
    }
}
