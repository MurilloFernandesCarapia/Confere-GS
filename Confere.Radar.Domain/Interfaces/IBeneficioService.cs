using Confere.Radar.Domain.Dtos.Beneficios;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IBeneficioService
    {
        Task<IReadOnlyList<BeneficioResumoResponse>> ListarAsync();
        Task<BeneficioDetalheResponse> ObterPorCodigoAsync(string codigo);
    }
}
