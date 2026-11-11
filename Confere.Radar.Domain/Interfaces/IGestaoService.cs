using Confere.Radar.Domain.Dtos.Gestao;
using Confere.Radar.Domain.Pagination;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IGestaoService
    {
        Task<PaginaResultado<IdentificadorGestaoResponse>> ListarIdentificadoresAsync(string? status, ParametrosPaginacao paginacao);
        Task<IdentificadorGestaoResponse> AtualizarStatusIdentificadorAsync(int id, AtualizarStatusIdentificadorRequest request);
        Task<PainelGestaoResponse> ObterPainelAsync(int? codIbge, DateTimeOffset? de, DateTimeOffset? ate, int? codIbgeGestor);
    }
}
