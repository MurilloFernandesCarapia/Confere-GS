using System.Security.Claims;
using Confere.Radar.Api.Extensions;
using Confere.Radar.Api.Security;
using Confere.Radar.Domain.Dtos.AlertasRegionais;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Confere.Radar.Api.Endpoints
{
    public static class AlertaRegionalEndpoints
    {
        public static IEndpointRouteBuilder MapAlertaRegionalEndpoints(this IEndpointRouteBuilder app)
        {
            var grupo = app.MapGroup("/alertas-regionais")
                .WithTags("Alertas regionais")
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            grupo.MapGet("", ListarAtivosAsync)
                .RequireAuthorization(Politicas.CidadaoOuGestor)
                .WithName("ListarAlertasRegionais")
                .WithSummary("Lista os alertas ativos de um município")
                .WithDescription("Se codIbge não for informado, usa o município que está no token. Alertas vencidos ou encerrados não aparecem.")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapGet("/{id:int}", ObterAsync)
                .RequireAuthorization(Politicas.CidadaoOuGestor)
                .WithName("ObterAlertaRegional")
                .WithSummary("Detalha um alerta regional")
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapPost("", CriarAsync)
                .RequireAuthorization(Politicas.Gestor)
                .ComValidacao<AlertaRegionalRequest>()
                .WithName("CriarAlertaRegional")
                .WithSummary("Cria um alerta regional")
                .WithDescription("Só o gestor do município pode criar. Se dtInicio não for enviado, o alerta começa agora.")
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapPut("/{id:int}", AtualizarAsync)
                .RequireAuthorization(Politicas.Gestor)
                .ComValidacao<AtualizarAlertaRegionalRequest>()
                .WithName("AtualizarAlertaRegional")
                .WithSummary("Edita um alerta regional")
                .WithDescription("Também vale para os alertas automáticos (RN14). Alerta encerrado não pode ser editado.")
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict);

            grupo.MapDelete("/{id:int}", EncerrarAsync)
                .RequireAuthorization(Politicas.Gestor)
                .WithName("EncerrarAlertaRegional")
                .WithSummary("Encerra um alerta regional")
                .WithDescription("O alerta não é apagado do banco, ele só fica inativo para manter o histórico do painel.")
                .ProducesProblem(StatusCodes.Status404NotFound);

            return app;
        }

        private static async Task<Ok<IReadOnlyList<AlertaRegionalResponse>>> ListarAtivosAsync(int? codIbge, ClaimsPrincipal usuario, IAlertaRegionalService alertaService)
        {
            var codigo = codIbge ?? usuario.ObterCodIbge()
                ?? throw new ValidacaoException("codIbge", "Informe o código IBGE do município.");

            var alertas = await alertaService.ListarAtivosAsync(codigo);
            return TypedResults.Ok(alertas);
        }

        private static async Task<Ok<AlertaRegionalResponse>> ObterAsync(int id, IAlertaRegionalService alertaService)
        {
            var alerta = await alertaService.ObterPorIdAsync(id);
            return TypedResults.Ok(alerta);
        }

        private static async Task<Created<AlertaRegionalResponse>> CriarAsync(AlertaRegionalRequest request, ClaimsPrincipal usuario, IAlertaRegionalService alertaService)
        {
            var alerta = await alertaService.CriarAsync(request, usuario.ObterIdGestor(), usuario.ObterCodIbge());
            return TypedResults.Created($"/alertas-regionais/{alerta.Id}", alerta);
        }

        private static async Task<Ok<AlertaRegionalResponse>> AtualizarAsync(int id, AtualizarAlertaRegionalRequest request, ClaimsPrincipal usuario, IAlertaRegionalService alertaService)
        {
            var alerta = await alertaService.AtualizarAsync(id, request, usuario.ObterIdGestor(), usuario.ObterCodIbge());
            return TypedResults.Ok(alerta);
        }

        private static async Task<NoContent> EncerrarAsync(int id, ClaimsPrincipal usuario, IAlertaRegionalService alertaService)
        {
            await alertaService.EncerrarAsync(id, usuario.ObterCodIbge());
            return TypedResults.NoContent();
        }
    }
}
