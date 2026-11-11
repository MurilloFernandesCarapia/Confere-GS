using Confere.Radar.Domain.Entities;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IAlertaRegionalRepository
    {
        Task<AlertaRegional?> ObterPorIdAsync(int id);
        Task<IReadOnlyList<AlertaRegional>> ListarAtivosAsync(int idMunicipio, DateTime agora);
        Task<bool> ExisteAtivoAsync(int idMunicipio, int idTipoGolpe, DateTime agora);
        Task<int> ContarEmitidosAsync(int idMunicipio, DateTime de, DateTime ate);
        Task AdicionarAsync(AlertaRegional alerta);
    }
}
