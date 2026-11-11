using Confere.Radar.Domain.Entities;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IGestorRepository
    {
        Task<Gestor?> ObterPorEmailAsync(string email);
    }
}
