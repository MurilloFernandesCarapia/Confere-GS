namespace Confere.Radar.Infrastructure.Security
{
    public class JwtSettings
    {
        public const string Secao = "Jwt";

        public string Secret { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string IssuerJava { get; set; } = string.Empty;
        public string IssuerRadar { get; set; } = string.Empty;
        public int ExpiracaoHoras { get; set; } = 8;
    }
}
