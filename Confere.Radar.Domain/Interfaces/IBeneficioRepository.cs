using Confere.Radar.Domain.Entities;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IBeneficioRepository
    {
        Task<IReadOnlyList<Beneficio>> ListarAtivosAsync();
        Task<Beneficio?> ObterPorCodigoAsync(string codigo);
    }
}
