using System.Security.Claims;
using Confere.Radar.Api.Extensions;
using Confere.Radar.Api.Security;
using Confere.Radar.Domain.Dtos.Auth;
using Confere.Radar.Domain.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Confere.Radar.Api.Endpoints
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app, IWebHostEnvironment ambiente)
        {
            var grupo = app.MapGroup("/auth").WithTags("Autenticação");

            grupo.MapPost("/registrar", RegistrarCidadaoAsync)
                .AllowAnonymous()
                .ComValidacao<RegistroCidadaoRequest>()
                .WithName("RegistrarCidadao")
                .WithSummary("Cadastro do cidadão")
                .WithDescription("Cria a conta do cidadão na API .NET. A senha precisa ter 8 caracteres, com letra e número. Se a pessoa já entrou antes com a conta da API Java, o cadastro só define a senha dela aqui.")
                .ProducesProblem(StatusCodes.Status409Conflict);

            grupo.MapPost("/login", LoginCidadaoAsync)
                .AllowAnonymous()
                .ComValidacao<LoginRequest>()
                .WithName("LoginCidadao")
                .WithSummary("Login do cidadão")
                .WithDescription("Devolve um JWT emitido pela API .NET (iss = confere-radar), válido por 8 horas.")
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            grupo.MapPost("/token", LoginGestorAsync)
                .AllowAnonymous()
                .ComValidacao<LoginRequest>()
                .WithName("LoginGestor")
                .WithSummary("Login do gestor público")
                .WithDescription("Autentica um gestor (CRAS, Procon, prefeitura) e devolve um JWT com o papel GESTOR.")
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            grupo.MapGet("/me", ObterCidadaoLogadoAsync)
                .RequireAuthorization(Politicas.Cidadao)
                .WithName("ObterCidadaoLogado")
                .WithSummary("Dados do cidadão logado")
                .WithDescription("Funciona com o token emitido por esta API ou pela API Java.")
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            if (ambiente.IsDevelopment())
            {
                grupo.MapPost("/dev/token-cidadao-java", GerarTokenCidadaoTeste)
                    .AllowAnonymous()
                    .WithName("GerarTokenCidadaoJavaTeste")
                    .WithSummary("[Somente desenvolvimento] Simula um token emitido pela API Java")
                    .WithDescription("Serve para demonstrar a integração: o token tem iss = confere-java e, no primeiro uso, a API cria o cidadão aqui pelo e-mail. Não existe fora do ambiente Development.");
            }

            return app;
        }

        private static async Task<Created<UsuarioLogadoResponse>> RegistrarCidadaoAsync(RegistroCidadaoRequest request, IAuthService authService)
        {
            var usuario = await authService.RegistrarCidadaoAsync(request);
            return TypedResults.Created("/auth/me", usuario);
        }

        private static async Task<Ok<TokenResponse>> LoginCidadaoAsync(LoginRequest request, IAuthService authService)
        {
            var token = await authService.LoginCidadaoAsync(request);
            return TypedResults.Ok(token);
        }

        private static async Task<Ok<TokenResponse>> LoginGestorAsync(LoginRequest request, IAuthService authService)
        {
            var token = await authService.LoginGestorAsync(request);
            return TypedResults.Ok(token);
        }

        private static async Task<Ok<UsuarioLogadoResponse>> ObterCidadaoLogadoAsync(ClaimsPrincipal usuario, IAuthService authService)
        {
            var cidadao = await authService.ObterCidadaoAsync(usuario.ObterIdUsuario());
            return TypedResults.Ok(cidadao);
        }

        private static Ok<TokenCidadaoTesteResponse> GerarTokenCidadaoTeste(TokenCidadaoTesteRequest request, ITokenService tokenService)
        {
            return TypedResults.Ok(tokenService.GerarTokenCidadaoTeste(request));
        }
    }
}
