using Confere.Radar.Application.Common;
using Confere.Radar.Application.Mappers;
using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Dtos.Radar;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Domain.Pagination;
using Microsoft.Extensions.Logging;

namespace Confere.Radar.Application.Services
{
    public class DenunciaService : IDenunciaService
    {
        private static readonly TimeSpan JanelaDuplicidade = TimeSpan.FromHours(24);

        private readonly IDenunciaRepository _denunciaRepository;
        private readonly IIdentificadorRepository _identificadorRepository;
        private readonly IMunicipioRepository _municipioRepository;
        private readonly ITipoGolpeRepository _tipoGolpeRepository;
        private readonly INormalizadorIdentificador _normalizador;
        private readonly IRadarScoreService _scoreService;
        private readonly IAlertaRegionalService _alertaRegionalService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<DenunciaService> _logger;

        public DenunciaService(
            IDenunciaRepository denunciaRepository,
            IIdentificadorRepository identificadorRepository,
            IMunicipioRepository municipioRepository,
            ITipoGolpeRepository tipoGolpeRepository,
            INormalizadorIdentificador normalizador,
            IRadarScoreService scoreService,
            IAlertaRegionalService alertaRegionalService,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider,
            ILogger<DenunciaService> logger)
        {
            _denunciaRepository = denunciaRepository;
            _identificadorRepository = identificadorRepository;
            _municipioRepository = municipioRepository;
            _tipoGolpeRepository = tipoGolpeRepository;
            _normalizador = normalizador;
            _scoreService = scoreService;
            _alertaRegionalService = alertaRegionalService;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<DenunciaCriadaResponse> CriarAsync(DenunciaRequest request, int idUsuario)
        {
            var municipio = await _municipioRepository.ObterPorCodIbgeAsync(request.CodIbgeMunicipio)
                ?? throw new ValidacaoException("codIbgeMunicipio", "Município não encontrado.");

            var tipoGolpe = await ObterTipoGolpeAsync(request.TipoGolpeCodigo);
            var normalizado = _normalizador.Normalizar(request.Tipo, request.Valor);
            var agora = _timeProvider.GetUtcNow().UtcDateTime;

            var identificador = await _identificadorRepository.ObterPorValorAsync(normalizado.Tipo, normalizado.Hash);

            if (identificador is null)
            {
                identificador = new IdentificadorSuspeito
                {
                    Tipo = normalizado.Tipo,
                    ValorHash = normalizado.Hash,
                    ValorMascarado = normalizado.ValorMascarado,
                    Status = StatusIdentificador.EmAnalise,
                    DtPrimeira = agora,
                    DtUltima = agora
                };

                await _identificadorRepository.AdicionarAsync(identificador);
            }
            else if (await _denunciaRepository.ExisteDoUsuarioDesdeAsync(idUsuario, identificador.IdIdentificador, agora - JanelaDuplicidade))
            {
                throw new ConflitoException("Você já denunciou esse identificador nas últimas 24 horas.", CodigoErro.DenunciaDuplicada);
            }

            var denuncia = new Denuncia
            {
                IdUsuario = idUsuario,
                Identificador = identificador,
                IdTipoGolpe = tipoGolpe.IdTipoGolpe,
                TipoGolpe = tipoGolpe,
                IdMunicipio = municipio.IdMunicipio,
                Municipio = municipio,
                IdVerificacaoExterna = request.IdVerificacao,
                Descricao = TextoOuNulo(request.Descricao),
                DtDenuncia = agora
            };

            await _denunciaRepository.AdicionarAsync(denuncia);
            await _unitOfWork.SalvarAsync();

            await _scoreService.AtualizarIndicadoresAsync(identificador, agora);
            await _unitOfWork.SalvarAsync();

            var alertaCriado = await _alertaRegionalService.AvaliarAlertaAutomaticoAsync(municipio, tipoGolpe);

            _logger.LogInformation("Denúncia {IdDenuncia} registrada para o identificador {IdIdentificador} (score {Score}, status {Status})",
                denuncia.IdDenuncia, identificador.IdIdentificador, identificador.Score, identificador.Status);

            return new DenunciaCriadaResponse(
                denuncia.IdDenuncia,
                RadarMapper.ParaResposta(identificador),
                alertaCriado,
                HorarioBrasilia.Converter(denuncia.DtDenuncia));
        }

        public async Task<PaginaResultado<DenunciaResponse>> ListarMinhasAsync(int idUsuario, ParametrosPaginacao paginacao)
        {
            var pagina = await _denunciaRepository.ListarDoUsuarioAsync(idUsuario, paginacao);
            return pagina.Converter(RadarMapper.ParaResposta);
        }

        public async Task<DenunciaResponse> ObterAsync(int id, int idUsuario)
        {
            var denuncia = await ObterDoUsuarioAsync(id, idUsuario);
            return RadarMapper.ParaResposta(denuncia);
        }

        public async Task<DenunciaResponse> AtualizarAsync(int id, AtualizarDenunciaRequest request, int idUsuario)
        {
            var denuncia = await ObterDoUsuarioAsync(id, idUsuario);
            var tipoGolpe = await ObterTipoGolpeAsync(request.TipoGolpeCodigo);
            var mudouTipo = denuncia.IdTipoGolpe != tipoGolpe.IdTipoGolpe;

            denuncia.IdTipoGolpe = tipoGolpe.IdTipoGolpe;
            denuncia.TipoGolpe = tipoGolpe;
            denuncia.Descricao = TextoOuNulo(request.Descricao);
            denuncia.DtAtualizacao = _timeProvider.GetUtcNow().UtcDateTime;

            await _unitOfWork.SalvarAsync();

            if (mudouTipo)
            {
                await _alertaRegionalService.AvaliarAlertaAutomaticoAsync(denuncia.Municipio!, tipoGolpe);
            }

            _logger.LogInformation("Denúncia {IdDenuncia} atualizada pelo usuário {IdUsuario}", denuncia.IdDenuncia, idUsuario);
            return RadarMapper.ParaResposta(denuncia);
        }

        public async Task ExcluirAsync(int id, int idUsuario)
        {
            var denuncia = await ObterDoUsuarioAsync(id, idUsuario);
            var identificador = denuncia.Identificador!;

            _denunciaRepository.Remover(denuncia);
            await _unitOfWork.SalvarAsync();

            await _scoreService.AtualizarIndicadoresAsync(identificador, _timeProvider.GetUtcNow().UtcDateTime);
            await _unitOfWork.SalvarAsync();

            _logger.LogInformation("Denúncia {IdDenuncia} excluída pelo usuário {IdUsuario}", id, idUsuario);
        }

        private async Task<Denuncia> ObterDoUsuarioAsync(int id, int idUsuario)
        {
            var denuncia = await _denunciaRepository.ObterPorIdAsync(id)
                ?? throw new NaoEncontradoException("Denúncia não encontrada.");

            if (denuncia.IdUsuario != idUsuario)
            {
                throw new SemPermissaoException("Você só pode ver e alterar as suas próprias denúncias.");
            }

            return denuncia;
        }

        private async Task<TipoGolpe> ObterTipoGolpeAsync(string codigo)
        {
            var tipoGolpe = await _tipoGolpeRepository.ObterPorCodigoAsync(codigo.Trim().ToUpperInvariant());
            return tipoGolpe ?? throw new ValidacaoException("tipoGolpeCodigo", "Tipo de golpe não encontrado. Consulte GET /radar/tipos-golpe.");
        }

        private static string? TextoOuNulo(string? texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
        }
    }
}
