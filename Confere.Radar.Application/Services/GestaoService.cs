using Confere.Radar.Application.Common;
using Confere.Radar.Application.Mappers;
using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Dtos.Gestao;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Domain.Pagination;
using Microsoft.Extensions.Logging;

namespace Confere.Radar.Application.Services
{
    public class GestaoService : IGestaoService
    {
        private const int DiasPadraoPainel = 30;
        private const int DiasMaximoPainel = 366;
        private const int QuantidadeTiposPainel = 5;

        private readonly IIdentificadorRepository _identificadorRepository;
        private readonly IDenunciaRepository _denunciaRepository;
        private readonly IAlertaRegionalRepository _alertaRepository;
        private readonly IMunicipioRepository _municipioRepository;
        private readonly IRadarScoreService _scoreService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<GestaoService> _logger;

        public GestaoService(
            IIdentificadorRepository identificadorRepository,
            IDenunciaRepository denunciaRepository,
            IAlertaRegionalRepository alertaRepository,
            IMunicipioRepository municipioRepository,
            IRadarScoreService scoreService,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider,
            ILogger<GestaoService> logger)
        {
            _identificadorRepository = identificadorRepository;
            _denunciaRepository = denunciaRepository;
            _alertaRepository = alertaRepository;
            _municipioRepository = municipioRepository;
            _scoreService = scoreService;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<PaginaResultado<IdentificadorGestaoResponse>> ListarIdentificadoresAsync(string? status, ParametrosPaginacao paginacao)
        {
            var statusNormalizado = string.IsNullOrWhiteSpace(status) ? null : status.Trim().ToUpperInvariant();

            if (statusNormalizado is not null && !StatusIdentificador.Todos.Contains(statusNormalizado))
            {
                throw new ValidacaoException("status", $"Status inválido. Use um destes: {string.Join(", ", StatusIdentificador.Todos)}.");
            }

            var pagina = await _identificadorRepository.ListarAsync(statusNormalizado, paginacao);
            return pagina.Converter(RadarMapper.ParaRespostaGestao);
        }

        public async Task<IdentificadorGestaoResponse> AtualizarStatusIdentificadorAsync(int id, AtualizarStatusIdentificadorRequest request)
        {
            var identificador = await _identificadorRepository.ObterPorIdAsync(id)
                ?? throw new NaoEncontradoException("Identificador não encontrado.");

            var novoStatus = request.Status.Trim().ToUpperInvariant();

            if (novoStatus == StatusIdentificador.Descartado)
            {
                identificador.Status = StatusIdentificador.Descartado;
            }
            else
            {
                identificador.Status = StatusIdentificador.EmAnalise;
                await _scoreService.AtualizarIndicadoresAsync(identificador, _timeProvider.GetUtcNow().UtcDateTime);
            }

            await _unitOfWork.SalvarAsync();

            _logger.LogInformation("Identificador {IdIdentificador} agora está {Status}", identificador.IdIdentificador, identificador.Status);
            return RadarMapper.ParaRespostaGestao(identificador);
        }

        public async Task<PainelGestaoResponse> ObterPainelAsync(int? codIbge, DateTimeOffset? de, DateTimeOffset? ate, int? codIbgeGestor)
        {
            var codigoMunicipio = codIbge ?? codIbgeGestor
                ?? throw new ValidacaoException("codIbge", "Informe o código IBGE do município.");

            if (codIbgeGestor != codigoMunicipio)
            {
                throw new SemPermissaoException("Você só pode ver o painel do município do seu órgão.");
            }

            var municipio = await _municipioRepository.ObterPorCodIbgeAsync(codigoMunicipio)
                ?? throw new NaoEncontradoException("Município não encontrado.");

            var fim = ate?.UtcDateTime ?? _timeProvider.GetUtcNow().UtcDateTime;
            var inicio = de?.UtcDateTime ?? fim.AddDays(-DiasPadraoPainel);

            if (inicio > fim)
            {
                throw new ValidacaoException("de", "A data inicial precisa ser antes da data final.");
            }

            if ((fim - inicio).TotalDays > DiasMaximoPainel)
            {
                throw new ValidacaoException("de", $"O período pode ter no máximo {DiasMaximoPainel} dias.");
            }

            var idMunicipio = municipio.IdMunicipio;

            var totalDenuncias = await _denunciaRepository.ContarNoPeriodoAsync(idMunicipio, inicio, fim);
            var confirmados = await _denunciaRepository.ContarIdentificadoresPorStatusAsync(idMunicipio, StatusIdentificador.Confirmado, inicio, fim);
            var emAnalise = await _denunciaRepository.ContarIdentificadoresPorStatusAsync(idMunicipio, StatusIdentificador.EmAnalise, inicio, fim);
            var descartados = await _denunciaRepository.ContarIdentificadoresPorStatusAsync(idMunicipio, StatusIdentificador.Descartado, inicio, fim);
            var alertasEmitidos = await _alertaRepository.ContarEmitidosAsync(idMunicipio, inicio, fim);
            var alertasAtivos = await _alertaRepository.ListarAtivosAsync(idMunicipio, _timeProvider.GetUtcNow().UtcDateTime);
            var tipos = await _denunciaRepository.ListarTiposMaisDenunciadosAsync(idMunicipio, inicio, fim, QuantidadeTiposPainel);

            return new PainelGestaoResponse(
                RadarMapper.ParaResposta(municipio),
                HorarioBrasilia.Converter(inicio),
                HorarioBrasilia.Converter(fim),
                totalDenuncias,
                confirmados,
                emAnalise,
                descartados,
                alertasEmitidos,
                alertasAtivos.Count,
                tipos);
        }
    }
}
