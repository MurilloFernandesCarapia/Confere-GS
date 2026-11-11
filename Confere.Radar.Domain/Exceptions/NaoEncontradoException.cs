using Confere.Radar.Domain.Constants;

namespace Confere.Radar.Domain.Exceptions
{
    public class NaoEncontradoException : ConfereException
    {
        public NaoEncontradoException(string mensagem) : base(CodigoErro.NaoEncontrado, mensagem)
        {
        }
    }
}
