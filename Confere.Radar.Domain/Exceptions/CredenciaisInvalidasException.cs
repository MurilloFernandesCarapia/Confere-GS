using Confere.Radar.Domain.Constants;

namespace Confere.Radar.Domain.Exceptions
{
    public class CredenciaisInvalidasException : ConfereException
    {
        public CredenciaisInvalidasException() : base(CodigoErro.NaoAutenticado, "E-mail ou senha inválidos.")
        {
        }
    }
}
