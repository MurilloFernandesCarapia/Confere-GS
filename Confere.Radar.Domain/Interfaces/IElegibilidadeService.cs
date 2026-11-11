using Confere.Radar.Domain.Dtos.Direitos;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Models;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IElegibilidadeService
    {
        ResultadoElegibilidade AvaliarBeneficio(Beneficio beneficio, PerfilSocial perfil);
        Task<IReadOnlyList<ElegibilidadeResponse>> AvaliarAsync(int idUsuario);
        Task<IReadOnlyList<ElegibilidadeResponse>> ListarAsync(int idUsuario);
        Task<ElegibilidadeResponse> AtualizarStatusAsync(int id, AtualizarElegibilidadeRequest request, int idUsuario);
    }
}
