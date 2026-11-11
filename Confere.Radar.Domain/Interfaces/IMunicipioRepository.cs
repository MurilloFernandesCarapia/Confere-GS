using Confere.Radar.Domain.Entities;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IMunicipioRepository
    {
        Task<Municipio?> ObterPorCodIbgeAsync(int codIbge);
    }
}
