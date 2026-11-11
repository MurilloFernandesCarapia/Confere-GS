using System.Security.Claims;
using Confere.Radar.Api.Extensions;
using Confere.Radar.Api.Security;
using Confere.Radar.Domain.Dtos.Radar;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Domain.Pagination;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Confere.Radar.Api.Endpoints
{
    public static class DenunciaEndpoints
    {
        public static IEndpointRouteBuilder MapDenunciaEndpoints(this IEndpointRouteBuilder app)
        {
            var grupo = app.MapGroup("/denuncias")
                .WithTags("Radar comunitário - Denúncias")
                .RequireAuthorization(Politicas.Cidadao)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            grupo.MapPost("", CriarAsync)
                .ComValidacao<DenunciaRequest>()
                .WithName("CriarDenuncia")
                .WithSummary("Registra uma denúncia")
                .WithDescription("Normaliza o identificador e guarda só o hash (RN11), recusa a mesma denúncia do mesmo usuário em 24 h (RN12), recalcula score e status (RN13) e cria alerta regional automático quando o município passa do limite (RN14).")
                .ProducesProblem(StatusCodes.Status409Conflict);

            grupo.MapGet("/minhas", ListarMinhasAsync)
                .WithName("ListarMinhasDenuncias")
                .WithSummary("Lista as denúncias do cidadão logado")
                .WithDescription("Paginado: pagina começa em 0 e tamanho vai até 50.");

            grupo.MapGet("/{id:int}", ObterAsync)
                .WithName("ObterDenuncia")
                .WithSummary("Detalha uma denúncia")
                .WithDescription("Só o dono da denúncia consegue ver.")
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapPut("/{id:int}", AtualizarAsync)
                .ComValidacao<AtualizarDenunciaRequest>()
                .WithName("AtualizarDenuncia")
                .WithSummary("Edita o tipo de golpe e a descrição")
                .WithDescription("O identificador denunciado não pode ser trocado. Para isso, exclua e crie outra denúncia.")
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapDelete("/{id:int}", ExcluirAsync)
                .WithName("ExcluirDenuncia")
                .WithSummary("Exclui uma denúncia")
                .WithDescription("Depois de excluir, o total e o score do identificador são recalculados.")
                .ProducesProblem(StatusCodes.Status404NotFound);

            return app;
        }

        private static async Task<Created<DenunciaCriadaResponse>> CriarAsync(DenunciaRequest request, ClaimsPrincipal usuario, IDenunciaService denunciaService)
        {
            var denuncia = await denunciaService.CriarAsync(request, usuario.ObterIdUsuario());
            return TypedResults.Created($"/denuncias/{denuncia.Id}", denuncia);
        }

        private static async Task<Ok<PaginaResultado<DenunciaResponse>>> ListarMinhasAsync(int? pagina, int? tamanho, ClaimsPrincipal usuario, IDenunciaService denunciaService)
        {
            var resultado = await denunciaService.ListarMinhasAsync(usuario.ObterIdUsuario(), new ParametrosPaginacao(pagina, tamanho));
            return TypedResults.Ok(resultado);
        }

        private static async Task<Ok<DenunciaResponse>> ObterAsync(int id, ClaimsPrincipal usuario, IDenunciaService denunciaService)
        {
            var denuncia = await denunciaService.ObterAsync(id, usuario.ObterIdUsuario());
            return TypedResults.Ok(denuncia);
        }

        private static async Task<Ok<DenunciaResponse>> AtualizarAsync(int id, AtualizarDenunciaRequest request, ClaimsPrincipal usuario, IDenunciaService denunciaService)
        {
            var denuncia = await denunciaService.AtualizarAsync(id, request, usuario.ObterIdUsuario());
            return TypedResults.Ok(denuncia);
        }

        private static async Task<NoContent> ExcluirAsync(int id, ClaimsPrincipal usuario, IDenunciaService denunciaService)
        {
            await denunciaService.ExcluirAsync(id, usuario.ObterIdUsuario());
            return TypedResults.NoContent();
        }
    }
}
