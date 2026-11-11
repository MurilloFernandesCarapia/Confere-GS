using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Domain.Models;
using Confere.Radar.Domain.Pagination;
using Confere.Radar.Infrastructure.Data;
using Confere.Radar.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Confere.Radar.Infrastructure.Repositories
{
    public class DenunciaRepository : IDenunciaRepository
    {
        private readonly AppDbContext _context;

        public DenunciaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Denuncia?> ObterPorIdAsync(int id)
        {
            return await _context.Denuncias
                .Include(d => d.Identificador)
                .Include(d => d.TipoGolpe)
                .Include(d => d.Municipio)
                .FirstOrDefaultAsync(d => d.IdDenuncia == id);
        }

        public async Task<PaginaResultado<Denuncia>> ListarDoUsuarioAsync(int idUsuario, ParametrosPaginacao paginacao)
        {
            return await _context.Denuncias
                .AsNoTracking()
                .Include(d => d.Identificador)
                .Include(d => d.TipoGolpe)
                .Include(d => d.Municipio)
                .Where(d => d.IdUsuario == idUsuario)
                .OrderByDescending(d => d.DtDenuncia)
                .PaginarAsync(paginacao);
        }

        public async Task<bool> ExisteDoUsuarioDesdeAsync(int idUsuario, int idIdentificador, DateTime desde)
        {
            return await _context.Denuncias.AnyAsync(d =>
                d.IdUsuario == idUsuario &&
                d.IdIdentificador == idIdentificador &&
                d.DtDenuncia >= desde);
        }

        public async Task<EstatisticaIdentificador> ObterEstatisticaAsync(int idIdentificador)
        {
            var denuncias = _context.Denuncias.Where(d => d.IdIdentificador == idIdentificador);

            var total = await denuncias.CountAsync();
            var municipios = await denuncias.Select(d => d.IdMunicipio).Distinct().CountAsync();
            var primeira = await denuncias.MinAsync(d => (DateTime?)d.DtDenuncia);
            var ultima = await denuncias.MaxAsync(d => (DateTime?)d.DtDenuncia);

            return new EstatisticaIdentificador(total, municipios, primeira, ultima);
        }

        public async Task<TipoGolpe?> ObterTipoMaisComumAsync(int idIdentificador)
        {
            var idTipoGolpe = await _context.Denuncias
                .Where(d => d.IdIdentificador == idIdentificador)
                .GroupBy(d => d.IdTipoGolpe)
                .Select(g => new { IdTipoGolpe = g.Key, Total = g.Count() })
                .OrderByDescending(g => g.Total)
                .Select(g => (int?)g.IdTipoGolpe)
                .FirstOrDefaultAsync();

            if (idTipoGolpe is null)
            {
                return null;
            }

            return await _context.TiposGolpe.FirstOrDefaultAsync(t => t.IdTipoGolpe == idTipoGolpe);
        }

        public async Task<int> ContarIdentificadoresConfirmadosAsync(int idMunicipio, int idTipoGolpe, DateTime desde)
        {
            return await _context.Denuncias
                .Where(d => d.IdMunicipio == idMunicipio &&
                            d.IdTipoGolpe == idTipoGolpe &&
                            d.DtDenuncia >= desde &&
                            d.Identificador!.Status == StatusIdentificador.Confirmado)
                .Select(d => d.IdIdentificador)
                .Distinct()
                .CountAsync();
        }

        public async Task<int> ContarNoPeriodoAsync(int idMunicipio, DateTime de, DateTime ate)
        {
            return await _context.Denuncias
                .CountAsync(d => d.IdMunicipio == idMunicipio && d.DtDenuncia >= de && d.DtDenuncia <= ate);
        }

        public async Task<int> ContarIdentificadoresPorStatusAsync(int idMunicipio, string status, DateTime de, DateTime ate)
        {
            return await _context.Denuncias
                .Where(d => d.IdMunicipio == idMunicipio &&
                            d.DtDenuncia >= de &&
                            d.DtDenuncia <= ate &&
                            d.Identificador!.Status == status)
                .Select(d => d.IdIdentificador)
                .Distinct()
                .CountAsync();
        }

        public async Task<IReadOnlyList<TotalPorTipoGolpe>> ListarTiposMaisDenunciadosAsync(int idMunicipio, DateTime de, DateTime ate, int quantidade)
        {
            var totais = await _context.Denuncias
                .Where(d => d.IdMunicipio == idMunicipio && d.DtDenuncia >= de && d.DtDenuncia <= ate)
                .GroupBy(d => new { d.TipoGolpe!.Codigo, d.TipoGolpe.Nome })
                .Select(g => new { g.Key.Codigo, g.Key.Nome, Total = g.Count() })
                .OrderByDescending(g => g.Total)
                .Take(quantidade)
                .ToListAsync();

            return totais.Select(t => new TotalPorTipoGolpe(t.Codigo, t.Nome, t.Total)).ToList();
        }

        public async Task AdicionarAsync(Denuncia denuncia)
        {
            await _context.Denuncias.AddAsync(denuncia);
        }

        public void Remover(Denuncia denuncia)
        {
            _context.Denuncias.Remove(denuncia);
        }
    }
}
