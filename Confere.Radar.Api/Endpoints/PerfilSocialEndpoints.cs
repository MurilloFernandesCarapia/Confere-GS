using System.Security.Claims;
using Confere.Radar.Api.Extensions;
using Confere.Radar.Api.Security;
using Confere.Radar.Domain.Dtos.Direitos;
using Confere.Radar.Domain.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Confere.Radar.Api.Endpoints
{
    public static class PerfilSocialEndpoints
    {
        public static IEndpointRouteBuilder MapPerfilSocialEndpoints(this IEndpointRouteBuilder app)
        {
            var grupo = app.MapGroup("/perfil-social")
                .WithTags("Meus direitos - Perfil social")
                .RequireAuthorization(Politicas.Cidadao)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            grupo.MapGet("", ObterAsync)
                .WithName("ObterPerfilSocial")
                .WithSummary("Mostra o meu perfil social")
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapPut("", SalvarAsync)
                .ComValidacao<PerfilSocialRequest>()
                .WithName("SalvarPerfilSocial")
                .WithSummary("Cria ou atualiza o meu perfil social")
                .WithDescription("Só grava com consentimento: true (RN17). A renda é informada por faixa (ATE_1_4, ATE_1_2, ATE_1, ATE_2, ACIMA_2), nunca o valor exato. Campos em branco significam \"não sei\" e deixam o resultado como POSSIVEL.");

            grupo.MapDelete("", ExcluirAsync)
                .WithName("ExcluirPerfilSocial")
                .WithSummary("Apaga o meu perfil social")
                .WithDescription("Também apaga os resultados calculados a partir dele. Os benefícios que você marcou como SOLICITADO ou CONCEDIDO continuam guardados.")
                .ProducesProblem(StatusCodes.Status404NotFound);

            return app;
        }

        private static async Task<Ok<PerfilSocialResponse>> ObterAsync(ClaimsPrincipal usuario, IPerfilSocialService perfilSocialService)
        {
            var perfil = await perfilSocialService.ObterAsync(usuario.ObterIdUsuario());
            return TypedResults.Ok(perfil);
        }

        private static async Task<Ok<PerfilSocialResponse>> SalvarAsync(PerfilSocialRequest request, ClaimsPrincipal usuario, IPerfilSocialService perfilSocialService)
        {
            var perfil = await perfilSocialService.SalvarAsync(request, usuario.ObterIdUsuario());
            return TypedResults.Ok(perfil);
        }

        private static async Task<NoContent> ExcluirAsync(ClaimsPrincipal usuario, IPerfilSocialService perfilSocialService)
        {
            await perfilSocialService.ExcluirAsync(usuario.ObterIdUsuario());
            return TypedResults.NoContent();
        }
    }
}
