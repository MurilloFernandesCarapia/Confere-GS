namespace Confere.Radar.Domain.Exceptions
{
    public abstract class ConfereException : Exception
    {
        public string Codigo { get; }

        protected ConfereException(string codigo, string mensagem) : base(mensagem)
        {
            Codigo = codigo;
        }
    }
}
