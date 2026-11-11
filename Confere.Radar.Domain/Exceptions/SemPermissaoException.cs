using Confere.Radar.Domain.Constants;

namespace Confere.Radar.Domain.Exceptions
{
    public class SemPermissaoException : ConfereException
    {
        public SemPermissaoException(string mensagem) : base(CodigoErro.SemPermissao, mensagem)
        {
        }
    }
}
