using Confere.Radar.Domain.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Confere.Radar.Infrastructure.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<PaginaResultado<T>> PaginarAsync<T>(this IQueryable<T> consulta, ParametrosPaginacao paginacao)
        {
            var totalItens = await consulta.CountAsync();

            var itens = await consulta
                .Skip(paginacao.Pagina * paginacao.Tamanho)
                .Take(paginacao.Tamanho)
                .ToListAsync();

            return new PaginaResultado<T>
            {
                Conteudo = itens,
                Pagina = paginacao.Pagina,
                Tamanho = paginacao.Tamanho,
                TotalItens = totalItens
            };
        }
    }
}
