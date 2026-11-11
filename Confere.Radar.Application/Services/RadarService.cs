using Confere.Radar.Application.Common;
using Confere.Radar.Application.Mappers;
using Confere.Radar.Domain.Dtos.Radar;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;

namespace Confere.Radar.Application.Services
{
    public class RadarService : IRadarService
    {
        private readonly IIdentificadorRepository _identificadorRepository;
        private readonly IDenunciaRepository _denunciaRepository;
        private readonly ITipoGolpeRepository _tipoGolpeRepository;
        private readonly INormalizadorIdentificador _normalizador;

        public RadarService(
            IIdentificadorRepository identificadorRepository,
            IDenunciaRepository denunciaRepository,
            ITipoGolpeRepository tipoGolpeRepository,
            INormalizadorIdentificador normalizador)
        {
            _identificadorRepository = identificadorRepository;
            _denunciaRepository = denunciaRepository;
            _tipoGolpeRepository = tipoGolpeRepository;
            _normalizador = normalizador;
        }

        public async Task<ConsultaRadarResponse> ConsultarAsync(string? tipo, string? valor)
        {
            if (string.IsNullOrWhiteSpace(tipo))
            {
                throw new ValidacaoException("tipo", "Informe o tipo: TELEFONE, LINK, CHAVE_PIX, EMAIL ou PERFIL.");
            }

            var normalizado = _normalizador.Normalizar(tipo, valor ?? string.Empty);
            var identificador = await _identificadorRepository.ObterPorValorAsync(normalizado.Tipo, normalizado.Hash);

            if (identificador is null || identificador.TotalDenuncias == 0)
            {
                return new ConsultaRadarResponse(false, null, 0, null, null);
            }

            var tipoMaisComum = await _denunciaRepository.ObterTipoMaisComumAsync(identificador.IdIdentificador);

            return new ConsultaRadarResponse(
                true,
                identificador.Status,
                identificador.TotalDenuncias,
                tipoMaisComum?.Nome,
                HorarioBrasilia.Converter(identificador.DtUltima));
        }

        public async Task<IReadOnlyList<TipoGolpeResponse>> ListarTiposGolpeAsync()
        {
            var tipos = await _tipoGolpeRepository.ListarAsync();
            return tipos.Select(RadarMapper.ParaResposta).ToList();
        }
    }
}
