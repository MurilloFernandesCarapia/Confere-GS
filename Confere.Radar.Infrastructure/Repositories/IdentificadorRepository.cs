using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Domain.Pagination;
using Confere.Radar.Infrastructure.Data;
using Confere.Radar.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Confere.Radar.Infrastructure.Repositories
{
    public class IdentificadorRepository : IIdentificadorRepository
    {
        private readonly AppDbContext _context;

        public IdentificadorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IdentificadorSuspeito?> ObterPorIdAsync(int id)
        {
            return await _context.Identificadores.FirstOrDefaultAsync(i => i.IdIdentificador == id);
        }

        public async Task<IdentificadorSuspeito?> ObterPorValorAsync(string tipo, string valorHash)
        {
            return await _context.Identificadores.FirstOrDefaultAsync(i => i.Tipo == tipo && i.ValorHash == valorHash);
        }

        public async Task<PaginaResultado<IdentificadorSuspeito>> ListarAsync(string? status, ParametrosPaginacao paginacao)
        {
            var consulta = _context.Identificadores.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status))
            {
                consulta = consulta.Where(i => i.Status == status);
            }

            return await consulta
                .OrderByDescending(i => i.Score)
                .ThenByDescending(i => i.DtUltima)
                .PaginarAsync(paginacao);
        }

        public async Task AdicionarAsync(IdentificadorSuspeito identificador)
        {
            await _context.Identificadores.AddAsync(identificador);
        }
    }
}
