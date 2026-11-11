namespace Confere.Radar.Domain.Constants
{
    public static class FaixaRenda
    {
        public const string AteUmQuarto = "ATE_1_4";
        public const string AteMeio = "ATE_1_2";
        public const string AteUm = "ATE_1";
        public const string AteDois = "ATE_2";
        public const string AcimaDeDois = "ACIMA_2";

        public static readonly IReadOnlyList<string> Todas = new[] { AteUmQuarto, AteMeio, AteUm, AteDois, AcimaDeDois };

        public static int Posicao(string faixa)
        {
            return Todas.ToList().IndexOf(faixa);
        }
    }
}
