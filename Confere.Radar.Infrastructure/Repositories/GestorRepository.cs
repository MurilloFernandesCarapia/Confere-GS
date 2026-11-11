using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Confere.Radar.Infrastructure.Repositories
{
    public class GestorRepository : IGestorRepository
    {
        private readonly AppDbContext _context;

        public GestorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Gestor?> ObterPorEmailAsync(string email)
        {
            var emailNormalizado = email.Trim().ToLowerInvariant();

            return await _context.Gestores
                .Include(g => g.Orgao)
                    .ThenInclude(o => o!.Municipio)
                .FirstOrDefaultAsync(g => g.Email == emailNormalizado);
        }
    }
}
