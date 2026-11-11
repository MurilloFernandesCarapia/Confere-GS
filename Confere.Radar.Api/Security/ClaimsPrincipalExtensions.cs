using System.Security.Claims;
using Confere.Radar.Domain.Exceptions;

namespace Confere.Radar.Api.Security
{
    public static class ClaimsPrincipalExtensions
    {
        public static int ObterIdUsuario(this ClaimsPrincipal usuario)
        {
            var id = usuario.FindFirst(ClaimsConfere.IdUsuarioLocal)?.Value ?? usuario.FindFirst(ClaimsConfere.Sub)?.Value;
            return ConverterId(id);
        }

        public static int ObterIdGestor(this ClaimsPrincipal usuario)
        {
            return ConverterId(usuario.FindFirst(ClaimsConfere.Sub)?.Value);
        }

        public static int? ObterCodIbge(this ClaimsPrincipal usuario)
        {
            return int.TryParse(usuario.FindFirst(ClaimsConfere.Municipio)?.Value, out var codIbge) ? codIbge : null;
        }

        private static int ConverterId(string? id)
        {
            if (!int.TryParse(id, out var valor))
            {
                throw new SemPermissaoException("O token não identifica o usuário.");
            }

            return valor;
        }
    }
}
