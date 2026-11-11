using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Confere.Radar.Application.Services
{
    public class IdempotenciaService : IIdempotenciaService
    {
        public static readonly TimeSpan Validade = TimeSpan.FromHours(24);

        private readonly IIdempotenciaRepository _idempotenciaRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<IdempotenciaService> _logger;

        public IdempotenciaService(
            IIdempotenciaRepository idempotenciaRepository,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider,
            ILogger<IdempotenciaService> logger)
        {
            _idempotenciaRepository = idempotenciaRepository;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<InicioIdempotencia> IniciarAsync(string chave, int idUsuario, string endpoint, string hashRequisicao)
        {
            var agora = _timeProvider.GetUtcNow().UtcDateTime;
            var registro = await _idempotenciaRepository.ObterAsync(chave);

            if (registro is not null && registro.DtExpiracao > agora)
            {
                if (registro.IdUsuario != idUsuario || registro.Endpoint != endpoint || registro.HashRequisicao != hashRequisicao)
                {
                    throw new IdempotenciaCorpoDiferenteException();
                }

                if (registro.Situacao == SituacaoIdempotencia.Processando)
                {
                    throw new ConflitoException("Já existe uma requisição com essa Idempotency-Key em processamento. Aguarde alguns segundos e tente de novo.",
                        CodigoErro.IdempotenciaEmProcessamento);
                }

                _logger.LogInformation("Requisição repetida com a Idempotency-Key {Chave}; devolvendo a resposta salva", chave);
                return InicioIdempotencia.Replay(registro.StatusHttp ?? 200, registro.CorpoResposta);
            }

            if (registro is null)
            {
                registro = new RegistroIdempotencia { Chave = chave };
                await _idempotenciaRepository.AdicionarAsync(registro);
            }

            registro.IdUsuario = idUsuario;
            registro.Endpoint = endpoint;
            registro.HashRequisicao = hashRequisicao;
            registro.Situacao = SituacaoIdempotencia.Processando;
            registro.StatusHttp = null;
            registro.CorpoResposta = null;
            registro.DtCriacao = agora;
            registro.DtExpiracao = agora + Validade;

            await _unitOfWork.SalvarAsync();

            return InicioIdempotencia.Nova();
        }

        public async Task ConcluirAsync(string chave, int statusHttp, string? corpoResposta)
        {
            var registro = await _idempotenciaRepository.ObterAsync(chave);
            if (registro is null)
            {
                return;
            }

            registro.Situacao = SituacaoIdempotencia.Concluida;
            registro.StatusHttp = statusHttp;
            registro.CorpoResposta = corpoResposta;

            await _unitOfWork.SalvarAsync();
        }

        public async Task CancelarAsync(string chave)
        {
            _unitOfWork.DescartarAlteracoes();

            var registro = await _idempotenciaRepository.ObterAsync(chave);
            if (registro is null || registro.Situacao != SituacaoIdempotencia.Processando)
            {
                return;
            }

            _idempotenciaRepository.Remover(registro);
            await _unitOfWork.SalvarAsync();

            _logger.LogInformation("A requisição com a Idempotency-Key {Chave} falhou; a chave foi liberada para uma nova tentativa", chave);
        }
    }
}
