using Confere.Radar.Api.Security;
using Confere.Radar.Domain.Dtos.Beneficios;
using Confere.Radar.Domain.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Confere.Radar.Api.Endpoints
{
    public static class BeneficioEndpoints
    {
        public static IEndpointRouteBuilder MapBeneficioEndpoints(this IEndpointRouteBuilder app)
        {
            var grupo = app.MapGroup("/beneficios")
                .WithTags("Meus direitos - Benefícios")
                .RequireAuthorization(Politicas.Cidadao)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            grupo.MapGet("", ListarAsync)
                .WithName("ListarBeneficios")
                .WithSummary("Lista o catálogo de benefícios ativos")
                .WithDescription("Mostra cada benefício em linguagem simples, com o valor de referência e o link oficial.");

            grupo.MapGet("/{codigo}", ObterPorCodigoAsync)
                .WithName("ObterBeneficio")
                .WithSummary("Detalha um benefício com requisitos e passo a passo")
                .WithDescription("Use o código do catálogo, por exemplo TARIFA_SOCIAL ou BPC_IDOSO.")
                .ProducesProblem(StatusCodes.Status404NotFound);

            return app;
        }

        private static async Task<Ok<IReadOnlyList<BeneficioResumoResponse>>> ListarAsync(IBeneficioService beneficioService)
        {
            var beneficios = await beneficioService.ListarAsync();
            return TypedResults.Ok(beneficios);
        }

        private static async Task<Ok<BeneficioDetalheResponse>> ObterPorCodigoAsync(string codigo, IBeneficioService beneficioService)
        {
            var beneficio = await beneficioService.ObterPorCodigoAsync(codigo);
            return TypedResults.Ok(beneficio);
        }
    }
}
