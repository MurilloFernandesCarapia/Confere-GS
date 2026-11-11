using Confere.Radar.Domain.Dtos.Radar;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IRadarService
    {
        Task<ConsultaRadarResponse> ConsultarAsync(string? tipo, string? valor);
        Task<IReadOnlyList<TipoGolpeResponse>> ListarTiposGolpeAsync();
    }
}
