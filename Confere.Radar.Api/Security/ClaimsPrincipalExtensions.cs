using System.Security.Claims;
using Confere.Radar.Domain.Exceptions;

namespace Confere.Radar.Api.Security
{
    public static class ClaimsPrincipalExtensions
    {
        public static int ObterIdUsuario(this ClaimsPrincipal usuario)
        {
            var id = usuario.FindFirst(ClaimsConfere.IdUsuarioLocal)?.Value ?? usuario.FindFirst(ClaimsConfere.Sub)?.Value;

            if (!int.TryParse(id, out var idUsuario))
            {
                throw new SemPermissaoException("O token não identifica o usuário.");
            }

            return idUsuario;
        }

        public static int? ObterCodIbge(this ClaimsPrincipal usuario)
        {
            return int.TryParse(usuario.FindFirst(ClaimsConfere.Municipio)?.Value, out var codIbge) ? codIbge : null;
        }
    }
}
