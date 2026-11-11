using Confere.Radar.Domain.Dtos.Auth;

namespace Confere.Radar.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<UsuarioLogadoResponse> RegistrarCidadaoAsync(RegistroCidadaoRequest request);
        Task<TokenResponse> LoginCidadaoAsync(LoginRequest request);
        Task<TokenResponse> LoginGestorAsync(LoginRequest request);
        Task<UsuarioLogadoResponse> ObterCidadaoAsync(int idUsuario);
        Task<int> ResolverCidadaoDaApiJavaAsync(string email, string nome, int? codIbgeMunicipio);
    }
}
