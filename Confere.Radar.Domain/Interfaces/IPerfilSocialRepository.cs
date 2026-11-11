using Confere.Radar.Domain.Entities;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IPerfilSocialRepository
    {
        Task<PerfilSocial?> ObterAsync(int idUsuario);
        Task AdicionarAsync(PerfilSocial perfil);
        void Remover(PerfilSocial perfil);
    }
}
