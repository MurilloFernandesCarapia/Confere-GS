using Confere.Radar.Domain.Entities;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IIdempotenciaRepository
    {
        Task<RegistroIdempotencia?> ObterAsync(string chave);
        Task AdicionarAsync(RegistroIdempotencia registro);
        void Remover(RegistroIdempotencia registro);
    }
}
