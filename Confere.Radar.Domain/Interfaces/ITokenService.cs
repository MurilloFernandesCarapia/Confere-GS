using Confere.Radar.Domain.Dtos.Auth;
using Confere.Radar.Domain.Entities;

namespace Confere.Radar.Domain.Interfaces
{
    public interface ITokenService
    {
        TokenResponse GerarTokenGestor(Gestor gestor);
        TokenResponse GerarTokenCidadao(Usuario usuario);
        TokenCidadaoTesteResponse GerarTokenCidadaoTeste(TokenCidadaoTesteRequest request);
    }
}
