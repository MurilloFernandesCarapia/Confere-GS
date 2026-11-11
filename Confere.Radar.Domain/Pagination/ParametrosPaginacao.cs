namespace Confere.Radar.Domain.Pagination
{
    public class ParametrosPaginacao
    {
        public const int TamanhoPadrao = 10;
        public const int TamanhoMaximo = 50;

        private int _pagina;
        private int _tamanho = TamanhoPadrao;

        public int Pagina
        {
            get => _pagina;
            set => _pagina = value < 0 ? 0 : value;
        }

        public int Tamanho
        {
            get => _tamanho;
            set => _tamanho = value <= 0 ? TamanhoPadrao : Math.Min(value, TamanhoMaximo);
        }

        public ParametrosPaginacao()
        {
        }

        public ParametrosPaginacao(int? pagina, int? tamanho)
        {
            Pagina = pagina ?? 0;
            Tamanho = tamanho ?? TamanhoPadrao;
        }
    }
}
