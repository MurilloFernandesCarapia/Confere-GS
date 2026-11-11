using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Confere.Radar.Infrastructure.Repositories
{
    public class ElegibilidadeRepository : IElegibilidadeRepository
    {
        private readonly AppDbContext _context;

        public ElegibilidadeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Elegibilidade>> ListarDoUsuarioAsync(int idUsuario)
        {
            return await _context.Elegibilidades
                .Include(e => e.Beneficio)
                .Where(e => e.IdUsuario == idUsuario)
                .OrderBy(e => e.Beneficio!.Nome)
                .ToListAsync();
        }

        public async Task<Elegibilidade?> ObterPorIdAsync(int id)
        {
            return await _context.Elegibilidades
                .Include(e => e.Beneficio)
                .FirstOrDefaultAsync(e => e.IdElegibilidade == id);
        }

        public async Task AdicionarAsync(Elegibilidade elegibilidade)
        {
            await _context.Elegibilidades.AddAsync(elegibilidade);
        }

        public void Remover(Elegibilidade elegibilidade)
        {
            _context.Elegibilidades.Remove(elegibilidade);
        }

        public async Task<int> ContarDestravadosAsync(int idMunicipio, DateTime de, DateTime ate)
        {
            return await (from elegibilidade in _context.Elegibilidades
                          join perfil in _context.PerfisSociais on elegibilidade.IdUsuario equals perfil.IdUsuario
                          where perfil.IdMunicipio == idMunicipio &&
                                (elegibilidade.Status == StatusElegibilidade.Solicitado || elegibilidade.Status == StatusElegibilidade.Concedido) &&
                                elegibilidade.DtAtualizacao >= de &&
                                elegibilidade.DtAtualizacao <= ate
                          select elegibilidade.IdElegibilidade)
                .CountAsync();
        }
    }
}
