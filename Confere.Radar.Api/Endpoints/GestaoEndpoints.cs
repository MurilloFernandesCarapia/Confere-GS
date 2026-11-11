using System.Security.Claims;
using Confere.Radar.Api.Extensions;
using Confere.Radar.Api.Security;
using Confere.Radar.Domain.Dtos.Gestao;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Domain.Pagination;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Confere.Radar.Api.Endpoints
{
    public static class GestaoEndpoints
    {
        public static IEndpointRouteBuilder MapGestaoEndpoints(this IEndpointRouteBuilder app)
        {
            var grupo = app.MapGroup("/gestao")
                .WithTags("Gestão pública")
                .RequireAuthorization(Politicas.Gestor)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            grupo.MapGet("/identificadores", ListarIdentificadoresAsync)
                .WithName("ListarIdentificadores")
                .WithSummary("Lista os identificadores denunciados")
                .WithDescription("Ordenado pelo score. Filtre por status (EM_ANALISE, CONFIRMADO ou DESCARTADO). O valor aparece sempre mascarado.")
                .ProducesProblem(StatusCodes.Status400BadRequest);

            grupo.MapPatch("/identificadores/{id:int}/status", AtualizarStatusAsync)
                .ComValidacao<AtualizarStatusIdentificadorRequest>()
                .WithName("AtualizarStatusIdentificador")
                .WithSummary("Marca um identificador como falso positivo ou reabre a análise")
                .WithDescription("DESCARTADO não volta a CONFIRMADO sozinho (RN13). EM_ANALISE reabre e recalcula o score na hora.")
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapGet("/painel", ObterPainelAsync)
                .WithName("ObterPainelGestao")
                .WithSummary("Indicadores do município")
                .WithDescription("Por padrão mostra o município do órgão do gestor nos últimos 30 dias. Datas no formato 2026-11-01 ou 2026-11-01T00:00:00-03:00.")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            return app;
        }

        private static async Task<Ok<PaginaResultado<IdentificadorGestaoResponse>>> ListarIdentificadoresAsync(string? status, int? pagina, int? tamanho, IGestaoService gestaoService)
        {
            var resultado = await gestaoService.ListarIdentificadoresAsync(status, new ParametrosPaginacao(pagina, tamanho));
            return TypedResults.Ok(resultado);
        }

        private static async Task<Ok<IdentificadorGestaoResponse>> AtualizarStatusAsync(int id, AtualizarStatusIdentificadorRequest request, IGestaoService gestaoService)
        {
            var identificador = await gestaoService.AtualizarStatusIdentificadorAsync(id, request);
            return TypedResults.Ok(identificador);
        }

        private static async Task<Ok<PainelGestaoResponse>> ObterPainelAsync(int? codIbge, DateTimeOffset? de, DateTimeOffset? ate, ClaimsPrincipal usuario, IGestaoService gestaoService)
        {
            var painel = await gestaoService.ObterPainelAsync(codIbge, de, ate, usuario.ObterCodIbge());
            return TypedResults.Ok(painel);
        }
    }
}
