using Confere.Radar.Domain.Constants;

namespace Confere.Radar.Domain.Exceptions
{
    public class ValidacaoException : ConfereException
    {
        public IReadOnlyList<ErroCampo> Erros { get; }

        public ValidacaoException(string campo, string mensagem)
            : this(new List<ErroCampo> { new ErroCampo(campo, mensagem) })
        {
        }

        public ValidacaoException(IReadOnlyList<ErroCampo> erros)
            : base(CodigoErro.Validacao, "Um ou mais campos estão inválidos.")
        {
            Erros = erros;
        }
    }
}
