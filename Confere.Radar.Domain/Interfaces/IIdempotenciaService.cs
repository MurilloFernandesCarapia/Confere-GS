using Confere.Radar.Domain.Models;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IIdempotenciaService
    {
        Task<InicioIdempotencia> IniciarAsync(string chave, int idUsuario, string endpoint, string hashRequisicao);
        Task ConcluirAsync(string chave, int statusHttp, string? corpoResposta);
        Task CancelarAsync(string chave);
    }
}
