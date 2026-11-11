using Confere.Radar.Application.Mappers;
using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Dtos.AlertasRegionais;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Confere.Radar.Application.Services
{
    public class AlertaRegionalService : IAlertaRegionalService
    {
        public const int MinimoIdentificadoresConfirmados = 3;

        private static readonly TimeSpan JanelaAnalise = TimeSpan.FromHours(48);
        private static readonly TimeSpan DuracaoAlertaAutomatico = TimeSpan.FromDays(7);

        private readonly IAlertaRegionalRepository _alertaRepository;
        private readonly IDenunciaRepository _denunciaRepository;
        private readonly IMunicipioRepository _municipioRepository;
        private readonly ITipoGolpeRepository _tipoGolpeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<AlertaRegionalService> _logger;

        public AlertaRegionalService(
            IAlertaRegionalRepository alertaRepository,
            IDenunciaRepository denunciaRepository,
            IMunicipioRepository municipioRepository,
            ITipoGolpeRepository tipoGolpeRepository,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider,
            ILogger<AlertaRegionalService> logger)
        {
            _alertaRepository = alertaRepository;
            _denunciaRepository = denunciaRepository;
            _municipioRepository = municipioRepository;
            _tipoGolpeRepository = tipoGolpeRepository;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<bool> AvaliarAlertaAutomaticoAsync(Municipio municipio, TipoGolpe tipoGolpe)
        {
            var agora = Agora();

            var confirmados = await _denunciaRepository.ContarIdentificadoresConfirmadosAsync(
                municipio.IdMunicipio, tipoGolpe.IdTipoGolpe, agora - JanelaAnalise);

            if (confirmados < MinimoIdentificadoresConfirmados)
            {
                return false;
            }

            if (await _alertaRepository.ExisteAtivoAsync(municipio.IdMunicipio, tipoGolpe.IdTipoGolpe, agora))
            {
                return false;
            }

            var alerta = new AlertaRegional
            {
                IdMunicipio = municipio.IdMunicipio,
                IdTipoGolpe = tipoGolpe.IdTipoGolpe,
                Titulo = Limitar($"Alerta em {municipio.Nome}: {tipoGolpe.Nome}", 120),
                Mensagem = Limitar($"A comunidade confirmou {confirmados} contatos diferentes aplicando o golpe \"{tipoGolpe.Nome}\" em {municipio.Nome} nas últimas 48 horas. Desconfie, não passe dados e confira no Confere antes de agir.", 600),
                Nivel = NivelAlerta.Atencao,
                Origem = OrigemAlerta.Automatico,
                DtInicio = agora,
                DtFim = agora + DuracaoAlertaAutomatico,
                Ativo = true
            };

            await _alertaRepository.AdicionarAsync(alerta);
            await _unitOfWork.SalvarAsync();

            _logger.LogInformation("Alerta regional automático {IdAlerta} criado para o município {IdMunicipio} e golpe {CodigoGolpe}",
                alerta.IdAlertaRegional, municipio.IdMunicipio, tipoGolpe.Codigo);

            return true;
        }

        public async Task<IReadOnlyList<AlertaRegionalResponse>> ListarAtivosAsync(int codIbgeMunicipio)
        {
            var municipio = await ObterMunicipioAsync(codIbgeMunicipio);
            var alertas = await _alertaRepository.ListarAtivosAsync(municipio.IdMunicipio, Agora());

            return alertas.Select(RadarMapper.ParaResposta).ToList();
        }

        public async Task<AlertaRegionalResponse> ObterPorIdAsync(int id)
        {
            var alerta = await ObterAlertaAsync(id);
            return RadarMapper.ParaResposta(alerta);
        }

        public async Task<AlertaRegionalResponse> CriarAsync(AlertaRegionalRequest request, int idGestor, int? codIbgeGestor)
        {
            GarantirMunicipioDoGestor(request.CodIbgeMunicipio, codIbgeGestor);

            var municipio = await ObterMunicipioAsync(request.CodIbgeMunicipio);
            var tipoGolpe = await ObterTipoGolpeAsync(request.TipoGolpeCodigo);

            var agora = Agora();
            var inicio = request.DtInicio?.UtcDateTime ?? agora;
            var fim = request.DtFim.UtcDateTime;
            ValidarPeriodo(inicio, fim, agora);

            var alerta = new AlertaRegional
            {
                IdMunicipio = municipio.IdMunicipio,
                Municipio = municipio,
                IdTipoGolpe = tipoGolpe.IdTipoGolpe,
                TipoGolpe = tipoGolpe,
                IdGestor = idGestor,
                Titulo = request.Titulo.Trim(),
                Mensagem = request.Mensagem.Trim(),
                Nivel = request.Nivel.Trim().ToUpperInvariant(),
                Origem = OrigemAlerta.Gestor,
                DtInicio = inicio,
                DtFim = fim,
                Ativo = true
            };

            await _alertaRepository.AdicionarAsync(alerta);
            await _unitOfWork.SalvarAsync();

            _logger.LogInformation("Gestor {IdGestor} criou o alerta regional {IdAlerta}", idGestor, alerta.IdAlertaRegional);
            return RadarMapper.ParaResposta(alerta);
        }

        public async Task<AlertaRegionalResponse> AtualizarAsync(int id, AtualizarAlertaRegionalRequest request, int idGestor, int? codIbgeGestor)
        {
            var alerta = await ObterAlertaAsync(id);
            GarantirMunicipioDoGestor(alerta.Municipio!.CodIbge, codIbgeGestor);

            if (!alerta.Ativo)
            {
                throw new ConflitoException("Esse alerta já foi encerrado e não pode mais ser editado.");
            }

            var tipoGolpe = await ObterTipoGolpeAsync(request.TipoGolpeCodigo);
            var fim = request.DtFim.UtcDateTime;
            ValidarPeriodo(alerta.DtInicio, fim, Agora());

            alerta.IdTipoGolpe = tipoGolpe.IdTipoGolpe;
            alerta.TipoGolpe = tipoGolpe;
            alerta.IdGestor = idGestor;
            alerta.Titulo = request.Titulo.Trim();
            alerta.Mensagem = request.Mensagem.Trim();
            alerta.Nivel = request.Nivel.Trim().ToUpperInvariant();
            alerta.DtFim = fim;

            await _unitOfWork.SalvarAsync();

            _logger.LogInformation("Gestor {IdGestor} editou o alerta regional {IdAlerta}", idGestor, alerta.IdAlertaRegional);
            return RadarMapper.ParaResposta(alerta);
        }

        public async Task EncerrarAsync(int id, int? codIbgeGestor)
        {
            var alerta = await ObterAlertaAsync(id);
            GarantirMunicipioDoGestor(alerta.Municipio!.CodIbge, codIbgeGestor);

            if (!alerta.Ativo)
            {
                return;
            }

            alerta.Ativo = false;
            await _unitOfWork.SalvarAsync();

            _logger.LogInformation("Alerta regional {IdAlerta} encerrado", alerta.IdAlertaRegional);
        }

        private static void GarantirMunicipioDoGestor(int codIbgeAlerta, int? codIbgeGestor)
        {
            if (codIbgeGestor is null || codIbgeGestor != codIbgeAlerta)
            {
                throw new SemPermissaoException("Você só pode gerenciar alertas do município do seu órgão.");
            }
        }

        private static void ValidarPeriodo(DateTime inicio, DateTime fim, DateTime agora)
        {
            if (fim <= inicio)
            {
                throw new ValidacaoException("dtFim", "A data de fim precisa ser depois da data de início.");
            }

            if (fim <= agora)
            {
                throw new ValidacaoException("dtFim", "A data de fim precisa estar no futuro.");
            }
        }

        private async Task<AlertaRegional> ObterAlertaAsync(int id)
        {
            var alerta = await _alertaRepository.ObterPorIdAsync(id);
            return alerta ?? throw new NaoEncontradoException("Alerta regional não encontrado.");
        }

        private async Task<Municipio> ObterMunicipioAsync(int codIbge)
        {
            var municipio = await _municipioRepository.ObterPorCodIbgeAsync(codIbge);
            return municipio ?? throw new NaoEncontradoException("Município não encontrado.");
        }

        private async Task<TipoGolpe> ObterTipoGolpeAsync(string codigo)
        {
            var tipoGolpe = await _tipoGolpeRepository.ObterPorCodigoAsync(codigo.Trim().ToUpperInvariant());
            return tipoGolpe ?? throw new ValidacaoException("tipoGolpeCodigo", "Tipo de golpe não encontrado. Consulte GET /radar/tipos-golpe.");
        }

        private DateTime Agora()
        {
            return _timeProvider.GetUtcNow().UtcDateTime;
        }

        private static string Limitar(string texto, int tamanho)
        {
            return texto.Length <= tamanho ? texto : texto[..(tamanho - 1)] + "…";
        }
    }
}
