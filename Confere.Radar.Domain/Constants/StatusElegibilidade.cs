namespace Confere.Radar.Domain.Constants
{
    public static class StatusElegibilidade
    {
        public const string Provavel = "PROVAVEL";
        public const string Possivel = "POSSIVEL";
        public const string Inelegivel = "INELEGIVEL";
        public const string Solicitado = "SOLICITADO";
        public const string Concedido = "CONCEDIDO";

        public static bool MarcadoPeloCidadao(string status)
        {
            return status == Solicitado || status == Concedido;
        }
    }
}
