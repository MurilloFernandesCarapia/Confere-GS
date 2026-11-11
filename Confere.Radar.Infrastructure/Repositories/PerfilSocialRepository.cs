using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Confere.Radar.Infrastructure.Repositories
{
    public class PerfilSocialRepository : IPerfilSocialRepository
    {
        private readonly AppDbContext _context;

        public PerfilSocialRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PerfilSocial?> ObterAsync(int idUsuario)
        {
            return await _context.PerfisSociais
                .Include(p => p.Municipio)
                .FirstOrDefaultAsync(p => p.IdUsuarioExterno == idUsuario);
        }

        public async Task AdicionarAsync(PerfilSocial perfil)
        {
            await _context.PerfisSociais.AddAsync(perfil);
        }

        public void Remover(PerfilSocial perfil)
        {
            _context.PerfisSociais.Remove(perfil);
        }
    }
}
