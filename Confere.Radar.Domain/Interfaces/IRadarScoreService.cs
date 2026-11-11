using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Models;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IRadarScoreService
    {
        int CalcularScore(EstatisticaIdentificador estatistica, DateTime agora);
        string DefinirStatus(string statusAtual, int totalDenuncias, int score);
        Task AtualizarIndicadoresAsync(IdentificadorSuspeito identificador, DateTime agora);
    }
}
