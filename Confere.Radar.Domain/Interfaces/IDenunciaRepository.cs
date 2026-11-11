using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Models;
using Confere.Radar.Domain.Pagination;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IDenunciaRepository
    {
        Task<Denuncia?> ObterPorIdAsync(int id);
        Task<PaginaResultado<Denuncia>> ListarDoUsuarioAsync(int idUsuario, ParametrosPaginacao paginacao);
        Task<bool> ExisteDoUsuarioDesdeAsync(int idUsuario, int idIdentificador, DateTime desde);
        Task<EstatisticaIdentificador> ObterEstatisticaAsync(int idIdentificador);
        Task<TipoGolpe?> ObterTipoMaisComumAsync(int idIdentificador);
        Task<int> ContarIdentificadoresConfirmadosAsync(int idMunicipio, int idTipoGolpe, DateTime desde);
        Task<int> ContarNoPeriodoAsync(int idMunicipio, DateTime de, DateTime ate);
        Task<int> ContarIdentificadoresPorStatusAsync(int idMunicipio, string status, DateTime de, DateTime ate);
        Task<IReadOnlyList<TotalPorTipoGolpe>> ListarTiposMaisDenunciadosAsync(int idMunicipio, DateTime de, DateTime ate, int quantidade);
        Task AdicionarAsync(Denuncia denuncia);
        void Remover(Denuncia denuncia);
    }
}
