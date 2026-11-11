using Confere.Radar.Domain.Constants;

namespace Confere.Radar.Domain.Exceptions
{
    public class ConsentimentoPendenteException : ConfereException
    {
        public ConsentimentoPendenteException(string mensagem) : base(CodigoErro.ConsentimentoPendente, mensagem)
        {
        }
    }
}
