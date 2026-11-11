using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Confere.Radar.Infrastructure.Repositories
{
    public class AlertaRegionalRepository : IAlertaRegionalRepository
    {
        private readonly AppDbContext _context;

        public AlertaRegionalRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AlertaRegional?> ObterPorIdAsync(int id)
        {
            return await _context.AlertasRegionais
                .Include(a => a.Municipio)
                .Include(a => a.TipoGolpe)
                .FirstOrDefaultAsync(a => a.IdAlertaRegional == id);
        }

        public async Task<IReadOnlyList<AlertaRegional>> ListarAtivosAsync(int idMunicipio, DateTime agora)
        {
            return await _context.AlertasRegionais
                .AsNoTracking()
                .Include(a => a.Municipio)
                .Include(a => a.TipoGolpe)
                .Where(a => a.IdMunicipio == idMunicipio && a.Ativo && a.DtInicio <= agora && a.DtFim > agora)
                .OrderByDescending(a => a.DtInicio)
                .ToListAsync();
        }

        public async Task<bool> ExisteAtivoAsync(int idMunicipio, int idTipoGolpe, DateTime agora)
        {
            return await _context.AlertasRegionais.AnyAsync(a =>
                a.IdMunicipio == idMunicipio &&
                a.IdTipoGolpe == idTipoGolpe &&
                a.Ativo &&
                a.DtFim > agora);
        }

        public async Task<int> ContarEmitidosAsync(int idMunicipio, DateTime de, DateTime ate)
        {
            return await _context.AlertasRegionais
                .CountAsync(a => a.IdMunicipio == idMunicipio && a.DtInicio >= de && a.DtInicio <= ate);
        }

        public async Task AdicionarAsync(AlertaRegional alerta)
        {
            await _context.AlertasRegionais.AddAsync(alerta);
        }
    }
}
