namespace Confere.Radar.Domain.Pagination
{
    public class PaginaResultado<T>
    {
        public IReadOnlyList<T> Conteudo { get; init; } = new List<T>();
        public int Pagina { get; init; }
        public int Tamanho { get; init; }
        public int TotalItens { get; init; }
        public int TotalPaginas => Tamanho <= 0 ? 0 : (int)Math.Ceiling(TotalItens / (double)Tamanho);

        public PaginaResultado<TDestino> Converter<TDestino>(Func<T, TDestino> conversor)
        {
            return new PaginaResultado<TDestino>
            {
                Conteudo = Conteudo.Select(conversor).ToList(),
                Pagina = Pagina,
                Tamanho = Tamanho,
                TotalItens = TotalItens
            };
        }
    }
}
