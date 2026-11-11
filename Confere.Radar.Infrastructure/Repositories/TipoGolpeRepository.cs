using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Confere.Radar.Infrastructure.Repositories
{
    public class TipoGolpeRepository : ITipoGolpeRepository
    {
        private readonly AppDbContext _context;

        public TipoGolpeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TipoGolpe>> ListarAsync()
        {
            return await _context.TiposGolpe
                .AsNoTracking()
                .OrderByDescending(t => t.Gravidade)
                .ThenBy(t => t.Nome)
                .ToListAsync();
        }

        public async Task<TipoGolpe?> ObterPorCodigoAsync(string codigo)
        {
            return await _context.TiposGolpe.FirstOrDefaultAsync(t => t.Codigo == codigo);
        }
    }
}
