using Confere.Radar.Application.Common;
using Confere.Radar.Domain.Dtos.Beneficios;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;

namespace Confere.Radar.Application.Services
{
    public class BeneficioService : IBeneficioService
    {
        private readonly IBeneficioRepository _beneficioRepository;

        public BeneficioService(IBeneficioRepository beneficioRepository)
        {
            _beneficioRepository = beneficioRepository;
        }

        public async Task<IReadOnlyList<BeneficioResumoResponse>> ListarAsync()
        {
            var beneficios = await _beneficioRepository.ListarAtivosAsync();
            return beneficios.Select(ParaResumo).ToList();
        }

        public async Task<BeneficioDetalheResponse> ObterPorCodigoAsync(string codigo)
        {
            var beneficio = await _beneficioRepository.ObterPorCodigoAsync(codigo.Trim().ToUpperInvariant());
            if (beneficio is null)
            {
                throw new NaoEncontradoException("Benefício não encontrado.");
            }

            var requisitos = beneficio.Requisitos
                .OrderBy(r => r.Grupo)
                .Select(r => new RequisitoBeneficioResponse(r.Grupo, r.Criterio, DescricaoRequisito.Descrever(r)))
                .ToList();

            var passos = beneficio.Passos
                .OrderBy(p => p.Ordem)
                .Select(p => new PassoBeneficioResponse(p.Ordem, p.Instrucao, p.Canal))
                .ToList();

            return new BeneficioDetalheResponse(
                beneficio.Codigo,
                beneficio.Nome,
                beneficio.OrgaoResponsavel,
                beneficio.DescricaoSimples,
                beneficio.ValorReferencia,
                beneficio.LinkOficial,
                requisitos,
                passos);
        }

        private static BeneficioResumoResponse ParaResumo(Beneficio beneficio)
        {
            return new BeneficioResumoResponse(
                beneficio.Codigo,
                beneficio.Nome,
                beneficio.OrgaoResponsavel,
                beneficio.DescricaoSimples,
                beneficio.ValorReferencia,
                beneficio.LinkOficial);
        }
    }
}
