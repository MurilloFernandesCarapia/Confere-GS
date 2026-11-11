using Confere.Radar.Domain.Dtos.AlertasRegionais;
using Confere.Radar.Domain.Entities;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IAlertaRegionalService
    {
        Task<bool> AvaliarAlertaAutomaticoAsync(Municipio municipio, TipoGolpe tipoGolpe);
        Task<IReadOnlyList<AlertaRegionalResponse>> ListarAtivosAsync(int codIbgeMunicipio);
        Task<AlertaRegionalResponse> ObterPorIdAsync(int id);
        Task<AlertaRegionalResponse> CriarAsync(AlertaRegionalRequest request, int idGestor, int? codIbgeGestor);
        Task<AlertaRegionalResponse> AtualizarAsync(int id, AtualizarAlertaRegionalRequest request, int idGestor, int? codIbgeGestor);
        Task EncerrarAsync(int id, int? codIbgeGestor);
    }
}
