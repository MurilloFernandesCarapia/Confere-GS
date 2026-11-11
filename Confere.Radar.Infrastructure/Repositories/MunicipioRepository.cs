using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Confere.Radar.Infrastructure.Repositories
{
    public class MunicipioRepository : IMunicipioRepository
    {
        private readonly AppDbContext _context;

        public MunicipioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Municipio?> ObterPorCodIbgeAsync(int codIbge)
        {
            return await _context.Municipios.FirstOrDefaultAsync(m => m.CodIbge == codIbge);
        }
    }
}
