using Confere.Radar.Application.Common;
using Confere.Radar.Application.Mappers;
using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Dtos.Direitos;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Confere.Radar.Application.Services
{
    public class PerfilSocialService : IPerfilSocialService
    {
        private readonly IPerfilSocialRepository _perfilSocialRepository;
        private readonly IElegibilidadeRepository _elegibilidadeRepository;
        private readonly IMunicipioRepository _municipioRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<PerfilSocialService> _logger;

        public PerfilSocialService(
            IPerfilSocialRepository perfilSocialRepository,
            IElegibilidadeRepository elegibilidadeRepository,
            IMunicipioRepository municipioRepository,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider,
            ILogger<PerfilSocialService> logger)
        {
            _perfilSocialRepository = perfilSocialRepository;
            _elegibilidadeRepository = elegibilidadeRepository;
            _municipioRepository = municipioRepository;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<PerfilSocialResponse> ObterAsync(int idUsuario)
        {
            var perfil = await ObterExistenteAsync(idUsuario);
            return ParaResposta(perfil);
        }

        public async Task<PerfilSocialResponse> SalvarAsync(PerfilSocialRequest request, int idUsuario)
        {
            if (!request.Consentimento)
            {
                throw new ConsentimentoPendenteException("Para salvar o perfil social, envie consentimento: true. Esses dados servem só para avaliar seus direitos.");
            }

            Municipio? municipio = null;
            if (request.CodIbgeMunicipio.HasValue)
            {
                municipio = await _municipioRepository.ObterPorCodIbgeAsync(request.CodIbgeMunicipio.Value)
                    ?? throw new ValidacaoException("codIbgeMunicipio", "Município não encontrado.");
            }

            var agora = _timeProvider.GetUtcNow().UtcDateTime;
            var perfil = await _perfilSocialRepository.ObterAsync(idUsuario);

            if (perfil is null)
            {
                perfil = new PerfilSocial { IdUsuario = idUsuario };
                await _perfilSocialRepository.AdicionarAsync(perfil);
            }

            perfil.Idade = request.Idade;
            perfil.RendaPcFaixa = request.RendaPcFaixa?.Trim().ToUpperInvariant();
            perfil.QtdMoradores = request.QtdMoradores;
            perfil.InscritoCadunico = request.InscritoCadunico;
            perfil.RecebeBpc = request.RecebeBpc;
            perfil.Deficiencia = request.Deficiencia;
            perfil.VinculoFormal = request.VinculoFormal;
            perfil.IdMunicipio = municipio?.IdMunicipio;
            perfil.Municipio = municipio;
            perfil.DtConsentimento = agora;
            perfil.DtAtualizacao = agora;

            await _unitOfWork.SalvarAsync();

            _logger.LogInformation("Perfil social do usuário {IdUsuario} salvo com consentimento", idUsuario);
            return ParaResposta(perfil);
        }

        public async Task ExcluirAsync(int idUsuario)
        {
            var perfil = await ObterExistenteAsync(idUsuario);
            var elegibilidades = await _elegibilidadeRepository.ListarDoUsuarioAsync(idUsuario);

            foreach (var elegibilidade in elegibilidades.Where(e => !StatusElegibilidade.MarcadoPeloCidadao(e.Status)))
            {
                _elegibilidadeRepository.Remover(elegibilidade);
            }

            _perfilSocialRepository.Remover(perfil);
            await _unitOfWork.SalvarAsync();

            _logger.LogInformation("Perfil social do usuário {IdUsuario} excluído", idUsuario);
        }

        private async Task<PerfilSocial> ObterExistenteAsync(int idUsuario)
        {
            return await _perfilSocialRepository.ObterAsync(idUsuario)
                ?? throw new NaoEncontradoException("Você ainda não preencheu o perfil social.");
        }

        private static PerfilSocialResponse ParaResposta(PerfilSocial perfil)
        {
            return new PerfilSocialResponse(
                perfil.Idade,
                perfil.RendaPcFaixa,
                perfil.RendaPcFaixa is null ? null : DescricaoRequisito.DescreverFaixa(perfil.RendaPcFaixa),
                perfil.QtdMoradores,
                perfil.InscritoCadunico,
                perfil.RecebeBpc,
                perfil.Deficiencia,
                perfil.VinculoFormal,
                perfil.Municipio is null ? null : RadarMapper.ParaResposta(perfil.Municipio),
                HorarioBrasilia.Converter(perfil.DtConsentimento),
                HorarioBrasilia.Converter(perfil.DtAtualizacao));
        }
    }
}
