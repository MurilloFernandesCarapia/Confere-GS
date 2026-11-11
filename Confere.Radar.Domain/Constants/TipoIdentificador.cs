namespace Confere.Radar.Domain.Constants
{
    public static class TipoIdentificador
    {
        public const string Telefone = "TELEFONE";
        public const string Link = "LINK";
        public const string ChavePix = "CHAVE_PIX";
        public const string Email = "EMAIL";
        public const string Perfil = "PERFIL";

        public static readonly IReadOnlyList<string> Todos = new[] { Telefone, Link, ChavePix, Email, Perfil };
    }
}
