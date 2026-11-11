using Confere.Radar.Domain.Constants;

namespace Confere.Radar.Domain.Exceptions
{
    public class ConflitoException : ConfereException
    {
        public ConflitoException(string mensagem, string codigo = CodigoErro.Conflito) : base(codigo, mensagem)
        {
        }
    }
}
