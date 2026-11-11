using Confere.Radar.Api.RateLimiting;
using Confere.Radar.Domain.Dtos.Radar;
using Confere.Radar.Domain.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Confere.Radar.Api.Endpoints
{
    public static class RadarEndpoints
    {
        public static IEndpointRouteBuilder MapRadarEndpoints(this IEndpointRouteBuilder app)
        {
            var grupo = app.MapGroup("/radar")
                .WithTags("Radar comunitário")
                .AllowAnonymous();

            grupo.MapGet("/consulta", ConsultarAsync)
                .RequireRateLimiting(PoliticasLimite.ConsultaPublica)
                .WithName("ConsultarRadar")
                .WithSummary("Consulta pública de um número, link, chave Pix, e-mail ou perfil")
                .WithDescription("Não precisa de login. Mostra só o status, o total de denúncias, o tipo de golpe mais comum e a data da última denúncia. Nunca mostra quem denunciou (RN15). Exemplo: tipo=TELEFONE e valor=11912345678. Limite de 20 consultas por minuto por IP (429 com Retry-After).")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status429TooManyRequests);

            grupo.MapGet("/tipos-golpe", ListarTiposGolpeAsync)
                .WithName("ListarTiposGolpe")
                .WithSummary("Lista os tipos de golpe do catálogo")
                .WithDescription("Use o campo codigo ao registrar uma denúncia ou um alerta regional.");

            return app;
        }

        private static async Task<Ok<ConsultaRadarResponse>> ConsultarAsync(string? tipo, string? valor, IRadarService radarService)
        {
            var resultado = await radarService.ConsultarAsync(tipo, valor);
            return TypedResults.Ok(resultado);
        }

        private static async Task<Ok<IReadOnlyList<TipoGolpeResponse>>> ListarTiposGolpeAsync(IRadarService radarService)
        {
            var tipos = await radarService.ListarTiposGolpeAsync();
            return TypedResults.Ok(tipos);
        }
    }
}
