namespace Confere.Radar.Api.RateLimiting
{
    public class LimitesRequisicaoSettings
    {
        public const string Secao = "RateLimiting";

        public LimiteJanela ConsultaPublica { get; set; } = new() { Limite = 20, JanelaSegundos = 60 };
        public LimiteTokens Denuncia { get; set; } = new();
        public LimiteJanela Global { get; set; } = new() { Limite = 100, JanelaSegundos = 60, Segmentos = 6 };
    }

    public class LimiteJanela
    {
        public int Limite { get; set; }
        public int JanelaSegundos { get; set; } = 60;
        public int Segmentos { get; set; } = 6;
    }

    public class LimiteTokens
    {
        public int Tokens { get; set; } = 5;
        public int TokensPorPeriodo { get; set; } = 5;
        public int PeriodoSegundos { get; set; } = 60;
    }
}
