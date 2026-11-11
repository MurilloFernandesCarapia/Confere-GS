using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Confere.Radar.Infrastructure.Repositories
{
    public class BeneficioRepository : IBeneficioRepository
    {
        private readonly AppDbContext _context;

        public BeneficioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Beneficio>> ListarAtivosAsync()
        {
            return await _context.Beneficios
                .AsNoTracking()
                .Include(b => b.Requisitos)
                .Where(b => b.Ativo)
                .OrderBy(b => b.Nome)
                .ToListAsync();
        }

        public async Task<Beneficio?> ObterPorCodigoAsync(string codigo)
        {
            return await _context.Beneficios
                .AsNoTracking()
                .Include(b => b.Requisitos)
                .Include(b => b.Passos.OrderBy(p => p.Ordem))
                .FirstOrDefaultAsync(b => b.Codigo == codigo && b.Ativo);
        }
    }
}
