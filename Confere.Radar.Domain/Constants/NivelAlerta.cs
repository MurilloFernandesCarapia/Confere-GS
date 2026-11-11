namespace Confere.Radar.Domain.Constants
{
    public static class NivelAlerta
    {
        public const string Info = "INFO";
        public const string Atencao = "ATENCAO";
        public const string Urgente = "URGENTE";

        public static readonly IReadOnlyList<string> Todos = new[] { Info, Atencao, Urgente };
    }
}
