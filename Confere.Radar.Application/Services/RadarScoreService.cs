using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Domain.Models;

namespace Confere.Radar.Application.Services
{
    public class RadarScoreService : IRadarScoreService
    {
        public const int PontosPorDenuncia = 12;
        public const int BonusDenunciaRecente = 20;
        public const int PontosPorMunicipioExtra = 10;
        public const int MinimoDenunciasConfirmacao = 5;
        public const int ScoreMinimoConfirmacao = 70;

        private static readonly TimeSpan JanelaRecente = TimeSpan.FromHours(48);

        private readonly IDenunciaRepository _denunciaRepository;

        public RadarScoreService(IDenunciaRepository denunciaRepository)
        {
            _denunciaRepository = denunciaRepository;
        }

        public int CalcularScore(EstatisticaIdentificador estatistica, DateTime agora)
        {
            if (estatistica.TotalDenuncias <= 0)
            {
                return 0;
            }

            var score = estatistica.TotalDenuncias * PontosPorDenuncia;

            if (estatistica.UltimaDenuncia.HasValue && agora - estatistica.UltimaDenuncia.Value < JanelaRecente)
            {
                score += BonusDenunciaRecente;
            }

            score += PontosPorMunicipioExtra * Math.Max(0, estatistica.MunicipiosDistintos - 1);

            return Math.Clamp(score, 0, 100);
        }

        public string DefinirStatus(string statusAtual, int totalDenuncias, int score)
        {
            if (statusAtual == StatusIdentificador.Descartado)
            {
                return StatusIdentificador.Descartado;
            }

            if (totalDenuncias >= MinimoDenunciasConfirmacao && score >= ScoreMinimoConfirmacao)
            {
                return StatusIdentificador.Confirmado;
            }

            if (statusAtual == StatusIdentificador.Confirmado && totalDenuncias >= MinimoDenunciasConfirmacao)
            {
                return StatusIdentificador.Confirmado;
            }

            return StatusIdentificador.EmAnalise;
        }

        public async Task AtualizarIndicadoresAsync(IdentificadorSuspeito identificador, DateTime agora)
        {
            var estatistica = await _denunciaRepository.ObterEstatisticaAsync(identificador.IdIdentificador);

            identificador.TotalDenuncias = estatistica.TotalDenuncias;
            identificador.Score = CalcularScore(estatistica, agora);
            identificador.Status = DefinirStatus(identificador.Status, identificador.TotalDenuncias, identificador.Score);

            if (estatistica.PrimeiraDenuncia.HasValue)
            {
                identificador.DtPrimeira = estatistica.PrimeiraDenuncia.Value;
            }

            if (estatistica.UltimaDenuncia.HasValue)
            {
                identificador.DtUltima = estatistica.UltimaDenuncia.Value;
            }
        }
    }
}
