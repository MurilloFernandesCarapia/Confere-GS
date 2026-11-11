using Confere.Radar.Domain.Constants;
using Confere.Radar.Api.Handlers;

namespace Confere.Radar.Api.Extensions
{
    public static class ProblemDetailsExtensions
    {
        public static IServiceCollection AddProblemDetailsConfere(this IServiceCollection services)
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();

            services.AddProblemDetails(options => options.CustomizeProblemDetails = contexto =>
            {
                var problema = contexto.ProblemDetails;
                var status = problema.Status ?? contexto.HttpContext.Response.StatusCode;

                if (!problema.Extensions.TryGetValue("codigo", out var codigo) || codigo is null)
                {
                    codigo = CodigoPorStatus(status);
                    problema.Extensions["codigo"] = codigo;
                    problema.Title = TituloPorStatus(status);
                }

                problema.Type = $"https://confere.app/erros/{codigo.ToString()!.ToLowerInvariant().Replace('_', '-')}";
                problema.Instance ??= contexto.HttpContext.Request.Path;
            });

            return services;
        }

        private static string CodigoPorStatus(int status)
        {
            return status switch
            {
                StatusCodes.Status400BadRequest => CodigoErro.Validacao,
                StatusCodes.Status401Unauthorized => CodigoErro.NaoAutenticado,
                StatusCodes.Status403Forbidden => CodigoErro.SemPermissao,
                StatusCodes.Status404NotFound => CodigoErro.NaoEncontrado,
                StatusCodes.Status405MethodNotAllowed => CodigoErro.MetodoNaoPermitido,
                StatusCodes.Status409Conflict => CodigoErro.Conflito,
                StatusCodes.Status429TooManyRequests => CodigoErro.LimiteExcedido,
                _ => CodigoErro.ErroInterno
            };
        }

        private static string TituloPorStatus(int status)
        {
            return status switch
            {
                StatusCodes.Status400BadRequest => "Dados inválidos",
                StatusCodes.Status401Unauthorized => "Não autenticado",
                StatusCodes.Status403Forbidden => "Sem permissão",
                StatusCodes.Status404NotFound => "Não encontrado",
                StatusCodes.Status405MethodNotAllowed => "Método não permitido",
                StatusCodes.Status409Conflict => "Conflito",
                StatusCodes.Status429TooManyRequests => "Limite de requisições excedido",
                _ => "Erro interno"
            };
        }
    }
}
