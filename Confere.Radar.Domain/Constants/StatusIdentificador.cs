namespace Confere.Radar.Domain.Constants
{
    public static class StatusIdentificador
    {
        public const string EmAnalise = "EM_ANALISE";
        public const string Confirmado = "CONFIRMADO";
        public const string Descartado = "DESCARTADO";

        public static readonly IReadOnlyList<string> Todos = new[] { EmAnalise, Confirmado, Descartado };
    }
}
