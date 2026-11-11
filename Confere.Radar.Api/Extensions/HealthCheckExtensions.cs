using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Confere.Radar.Api.Extensions
{
    public static class HealthCheckExtensions
    {
        private static readonly JsonSerializerOptions OpcoesJson = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
        {
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = registro => registro.Tags.Contains("live"),
                ResponseWriter = EscreverRespostaAsync
            });

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = registro => registro.Tags.Contains("ready"),
                ResponseWriter = EscreverRespostaAsync
            });

            return app;
        }

        private static Task EscreverRespostaAsync(HttpContext context, HealthReport relatorio)
        {
            var resposta = new
            {
                status = relatorio.Status.ToString(),
                duracaoTotalMs = Math.Round(relatorio.TotalDuration.TotalMilliseconds, 1),
                componentes = relatorio.Entries.Select(entrada => new
                {
                    nome = entrada.Key,
                    status = entrada.Value.Status.ToString(),
                    descricao = entrada.Value.Description,
                    duracaoMs = Math.Round(entrada.Value.Duration.TotalMilliseconds, 1)
                })
            };

            context.Response.ContentType = "application/json; charset=utf-8";
            return context.Response.WriteAsync(JsonSerializer.Serialize(resposta, OpcoesJson));
        }
    }
}
