using Confere.Radar.Domain.Entities;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IElegibilidadeRepository
    {
        Task<IReadOnlyList<Elegibilidade>> ListarDoUsuarioAsync(int idUsuario);
        Task<Elegibilidade?> ObterPorIdAsync(int id);
        Task AdicionarAsync(Elegibilidade elegibilidade);
        Task<int> ContarDestravadosAsync(int idMunicipio, DateTime de, DateTime ate);
    }
}
