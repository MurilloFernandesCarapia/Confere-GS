using Confere.Radar.Domain.Entities;

namespace Confere.Radar.Domain.Interfaces
{
    public interface ITipoGolpeRepository
    {
        Task<IReadOnlyList<TipoGolpe>> ListarAsync();
        Task<TipoGolpe?> ObterPorCodigoAsync(string codigo);
    }
}
