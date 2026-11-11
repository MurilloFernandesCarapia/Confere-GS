using Confere.Radar.Domain.Dtos.Direitos;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IPerfilSocialService
    {
        Task<PerfilSocialResponse> ObterAsync(int idUsuario);
        Task<PerfilSocialResponse> SalvarAsync(PerfilSocialRequest request, int idUsuario);
        Task ExcluirAsync(int idUsuario);
    }
}
