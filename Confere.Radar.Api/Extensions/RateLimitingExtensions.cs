using System.Globalization;
using System.Threading.RateLimiting;
using Confere.Radar.Api.RateLimiting;
using Confere.Radar.Api.Security;
using Confere.Radar.Domain.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Confere.Radar.Api.Extensions
{
    public static class RateLimitingExtensions
    {
        private static readonly string[] CaminhosLivres = { "/health", "/swagger" };

        public static IServiceCollection AddRateLimitingConfere(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<LimitesRequisicaoSettings>()
                .Bind(configuration.GetSection(LimitesRequisicaoSettings.Secao))
                .Validate(l => l.ConsultaPublica.Limite > 0 && l.Global.Limite > 0 && l.Denuncia.Tokens > 0,
                    "Os limites de RateLimiting precisam ser maiores que zero.")
                .ValidateOnStart();

            services.AddRateLimiter(_ => { });

            services.AddOptions<RateLimiterOptions>()
                .Configure<IOptions<LimitesRequisicaoSettings>>((options, limitesOptions) =>
                {
                    var limites = limitesOptions.Value;

                    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    {
                        if (CaminhosLivres.Any(c => httpContext.Request.Path.StartsWithSegments(c)))
                        {
                            return RateLimitPartition.GetNoLimiter("livre");
                        }

                        return RateLimitPartition.GetSlidingWindowLimiter(ChaveDoUsuario(httpContext), _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = limites.Global.Limite,
                            Window = TimeSpan.FromSeconds(limites.Global.JanelaSegundos),
                            SegmentsPerWindow = Math.Max(1, limites.Global.Segmentos),
                            QueueLimit = 0
                        });
                    });

                    options.AddPolicy(PoliticasLimite.ConsultaPublica, httpContext =>
                        RateLimitPartition.GetFixedWindowLimiter(ChaveDoIp(httpContext), _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = limites.ConsultaPublica.Limite,
                            Window = TimeSpan.FromSeconds(limites.ConsultaPublica.JanelaSegundos),
                            QueueLimit = 0
                        }));

                    options.AddPolicy(PoliticasLimite.Denuncia, httpContext =>
                        RateLimitPartition.GetTokenBucketLimiter(ChaveDoUsuario(httpContext), _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = limites.Denuncia.Tokens,
                            TokensPerPeriod = limites.Denuncia.TokensPorPeriodo,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(limites.Denuncia.PeriodoSegundos),
                            AutoReplenishment = true,
                            QueueLimit = 0
                        }));

                    options.OnRejected = (context, _) => ResponderLimiteExcedidoAsync(context);
                });

            return services;
        }

        private static async ValueTask ResponderLimiteExcedidoAsync(OnRejectedContext context)
        {
            var httpContext = context.HttpContext;

            var segundos = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var esperar)
                ? (int)Math.Ceiling(esperar.TotalSeconds)
                : 60;
            segundos = Math.Max(1, segundos);

            httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            httpContext.Response.Headers.RetryAfter = segundos.ToString(CultureInfo.InvariantCulture);

            var logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Confere.Radar.Api.RateLimiting");
            logger.LogWarning("Limite de requisições excedido em {Metodo} {Caminho} por {Chave}",
                httpContext.Request.Method, httpContext.Request.Path, ChaveDoUsuario(httpContext));

            var problema = new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Limite de requisições excedido",
                Detail = $"Você fez muitas requisições em pouco tempo. Tente de novo em {segundos} segundos.",
                Instance = httpContext.Request.Path
            };

            problema.Extensions["codigo"] = CodigoErro.LimiteExcedido;
            problema.Extensions["tentarNovamenteEmSegundos"] = segundos;

            var problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problema
            });
        }

        private static string ChaveDoUsuario(HttpContext httpContext)
        {
            var usuario = httpContext.User;

            if (usuario.Identity?.IsAuthenticated == true)
            {
                var id = usuario.FindFirst(ClaimsConfere.IdUsuarioLocal)?.Value ?? usuario.FindFirst(ClaimsConfere.Sub)?.Value;
                var papel = usuario.FindFirst(ClaimsConfere.Papel)?.Value ?? "SEM_PAPEL";
                return $"usuario:{papel}:{id}";
            }

            return ChaveDoIp(httpContext);
        }

        private static string ChaveDoIp(HttpContext httpContext)
        {
            return $"ip:{httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido"}";
        }
    }
}
