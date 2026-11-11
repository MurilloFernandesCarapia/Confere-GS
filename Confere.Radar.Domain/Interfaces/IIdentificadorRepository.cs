using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Pagination;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IIdentificadorRepository
    {
        Task<IdentificadorSuspeito?> ObterPorIdAsync(int id);
        Task<IdentificadorSuspeito?> ObterPorValorAsync(string tipo, string valorHash);
        Task<PaginaResultado<IdentificadorSuspeito>> ListarAsync(string? status, ParametrosPaginacao paginacao);
        Task AdicionarAsync(IdentificadorSuspeito identificador);
    }
}
