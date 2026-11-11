using Confere.Radar.Domain.Constants;

namespace Confere.Radar.Domain.Exceptions
{
    public class IdempotenciaCorpoDiferenteException : ConfereException
    {
        public IdempotenciaCorpoDiferenteException()
            : base(CodigoErro.IdempotenciaCorpoDiferente, "Essa Idempotency-Key já foi usada com um corpo de requisição diferente.")
        {
        }
    }
}
