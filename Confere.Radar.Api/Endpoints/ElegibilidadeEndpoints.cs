using System.Security.Claims;
using Confere.Radar.Api.Extensions;
using Confere.Radar.Api.Security;
using Confere.Radar.Domain.Dtos.Direitos;
using Confere.Radar.Domain.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Confere.Radar.Api.Endpoints
{
    public static class ElegibilidadeEndpoints
    {
        public static IEndpointRouteBuilder MapElegibilidadeEndpoints(this IEndpointRouteBuilder app)
        {
            var grupo = app.MapGroup("/elegibilidades")
                .WithTags("Meus direitos - Elegibilidade")
                .RequireAuthorization(Politicas.Cidadao)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            grupo.MapPost("/avaliar", AvaliarAsync)
                .WithName("AvaliarElegibilidade")
                .WithSummary("Avalia todos os benefícios ativos com base no meu perfil social")
                .WithDescription("Requisitos do mesmo grupo valem juntos (E) e grupos diferentes são alternativas (OU). Algum grupo atendido dá PROVAVEL, dado faltante ou benefício sem requisito dá POSSIVEL e nenhum grupo atendido dá INELEGIVEL (RN16). O que você marcou como SOLICITADO ou CONCEDIDO não é sobrescrito. Sem perfil social responde 403 CONSENTIMENTO_PENDENTE.");

            grupo.MapGet("", ListarAsync)
                .WithName("ListarElegibilidades")
                .WithSummary("Lista os meus resultados");

            grupo.MapPatch("/{id:int}", AtualizarStatusAsync)
                .ComValidacao<AtualizarElegibilidadeRequest>()
                .WithName("AtualizarElegibilidade")
                .WithSummary("Marca um benefício como SOLICITADO ou CONCEDIDO")
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict);

            return app;
        }

        private static async Task<Ok<IReadOnlyList<ElegibilidadeResponse>>> AvaliarAsync(ClaimsPrincipal usuario, IElegibilidadeService elegibilidadeService)
        {
            var resultados = await elegibilidadeService.AvaliarAsync(usuario.ObterIdUsuario());
            return TypedResults.Ok(resultados);
        }

        private static async Task<Ok<IReadOnlyList<ElegibilidadeResponse>>> ListarAsync(ClaimsPrincipal usuario, IElegibilidadeService elegibilidadeService)
        {
            var resultados = await elegibilidadeService.ListarAsync(usuario.ObterIdUsuario());
            return TypedResults.Ok(resultados);
        }

        private static async Task<Ok<ElegibilidadeResponse>> AtualizarStatusAsync(int id, AtualizarElegibilidadeRequest request, ClaimsPrincipal usuario, IElegibilidadeService elegibilidadeService)
        {
            var resultado = await elegibilidadeService.AtualizarStatusAsync(id, request, usuario.ObterIdUsuario());
            return TypedResults.Ok(resultado);
        }
    }
}
