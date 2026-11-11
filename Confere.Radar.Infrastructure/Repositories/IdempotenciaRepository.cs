using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Confere.Radar.Infrastructure.Repositories
{
    public class IdempotenciaRepository : IIdempotenciaRepository
    {
        private readonly AppDbContext _context;

        public IdempotenciaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RegistroIdempotencia?> ObterAsync(string chave)
        {
            return await _context.RegistrosIdempotencia.FirstOrDefaultAsync(r => r.Chave == chave);
        }

        public async Task AdicionarAsync(RegistroIdempotencia registro)
        {
            await _context.RegistrosIdempotencia.AddAsync(registro);
        }

        public void Remover(RegistroIdempotencia registro)
        {
            _context.RegistrosIdempotencia.Remove(registro);
        }
    }
}
