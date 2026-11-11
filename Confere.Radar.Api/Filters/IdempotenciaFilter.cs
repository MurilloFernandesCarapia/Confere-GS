using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Confere.Radar.Api.Security;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Confere.Radar.Api.Filters
{
    public class IdempotenciaFilter : IEndpointFilter
    {
        public const string HeaderChave = "Idempotency-Key";
        public const string HeaderReplay = "Idempotent-Replayed";

        private const string NamespaceDtos = "Confere.Radar.Domain.Dtos";

        private readonly IIdempotenciaService _idempotenciaService;
        private readonly JsonSerializerOptions _opcoesJson;

        public IdempotenciaFilter(IIdempotenciaService idempotenciaService, IOptions<JsonOptions> opcoesJson)
        {
            _idempotenciaService = idempotenciaService;
            _opcoesJson = opcoesJson.Value.SerializerOptions;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var httpContext = context.HttpContext;
            var chave = ObterChave(httpContext);
            var idUsuario = httpContext.User.ObterIdUsuario();
            var endpoint = DescreverEndpoint(httpContext);
            var hash = CalcularHash(context.Arguments);

            var inicio = await _idempotenciaService.IniciarAsync(chave, idUsuario, endpoint, hash);

            if (inicio.Repetida)
            {
                httpContext.Response.Headers[HeaderReplay] = "true";
                return Results.Content(inicio.CorpoResposta ?? string.Empty, "application/json; charset=utf-8", Encoding.UTF8, inicio.StatusHttp);
            }

            try
            {
                var resultado = await next(context);

                var statusHttp = (resultado as IStatusCodeHttpResult)?.StatusCode ?? StatusCodes.Status200OK;
                var valor = (resultado as IValueHttpResult)?.Value;
                var corpo = valor is null ? null : JsonSerializer.Serialize(valor, valor.GetType(), _opcoesJson);

                await _idempotenciaService.ConcluirAsync(chave, statusHttp, corpo);
                return resultado;
            }
            catch
            {
                await _idempotenciaService.CancelarAsync(chave);
                throw;
            }
        }

        private static string ObterChave(HttpContext httpContext)
        {
            var valor = httpContext.Request.Headers[HeaderChave].ToString();

            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new ValidacaoException(HeaderChave, "Envie o header Idempotency-Key com um UUID novo a cada operação. No PowerShell dá para gerar com [guid]::NewGuid().");
            }

            if (!Guid.TryParse(valor.Trim(), out var chave))
            {
                throw new ValidacaoException(HeaderChave, "O header Idempotency-Key precisa ser um UUID, por exemplo 6f1c2d0e-8a4b-4c1d-9e2f-3a5b7c9d1e2f.");
            }

            return chave.ToString("D");
        }

        private static string DescreverEndpoint(HttpContext httpContext)
        {
            var rota = (httpContext.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? httpContext.Request.Path.Value ?? string.Empty;
            return $"{httpContext.Request.Method} {rota}";
        }

        private string CalcularHash(IList<object?> argumentos)
        {
            var corpos = argumentos
                .Where(a => a is not null && a.GetType().Namespace?.StartsWith(NamespaceDtos) == true)
                .Select(a => JsonSerializer.Serialize(a, a!.GetType(), _opcoesJson));

            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", corpos)));
            return Convert.ToHexStringLower(bytes);
        }
    }
}
