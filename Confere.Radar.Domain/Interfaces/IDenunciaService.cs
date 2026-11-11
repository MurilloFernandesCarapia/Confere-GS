using Confere.Radar.Domain.Dtos.Radar;
using Confere.Radar.Domain.Pagination;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IDenunciaService
    {
        Task<DenunciaCriadaResponse> CriarAsync(DenunciaRequest request, int idUsuario);
        Task<PaginaResultado<DenunciaResponse>> ListarMinhasAsync(int idUsuario, ParametrosPaginacao paginacao);
        Task<DenunciaResponse> ObterAsync(int id, int idUsuario);
        Task<DenunciaResponse> AtualizarAsync(int id, AtualizarDenunciaRequest request, int idUsuario);
        Task ExcluirAsync(int id, int idUsuario);
    }
}
