using System.Security.Claims;
using System.Text;
using Confere.Radar.Api.Security;
using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Confere.Radar.Api.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddJwtConfere(this IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtSettings>>((options, jwtSettings) =>
                {
                    var settings = jwtSettings.Value;

                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuers = new[] { settings.IssuerRadar, settings.IssuerJava },
                        ValidateAudience = true,
                        ValidAudience = settings.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30),
                        NameClaimType = ClaimsConfere.Nome,
                        RoleClaimType = ClaimsConfere.Papel
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context => VincularUsuarioLocalAsync(context, settings),
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();

                            var detalhe = context.AuthenticateFailure is SecurityTokenExpiredException
                                ? "O token expirou. Faça login novamente."
                                : "Envie um token JWT válido no header Authorization: Bearer {token}.";

                            context.Response.Headers.WWWAuthenticate = "Bearer";
                            await EscreverProblemaAsync(context.HttpContext, StatusCodes.Status401Unauthorized,
                                CodigoErro.NaoAutenticado, "Não autenticado", detalhe);
                        },
                        OnForbidden = context => EscreverProblemaAsync(context.HttpContext, StatusCodes.Status403Forbidden,
                            CodigoErro.SemPermissao, "Sem permissão", "Seu perfil não tem permissão para acessar este recurso.")
                    };
                });

            services.AddAuthorizationBuilder()
                .AddPolicy(Politicas.Cidadao, politica => politica.RequireRole(Papel.Cidadao))
                .AddPolicy(Politicas.Gestor, politica => politica.RequireRole(Papel.Gestor))
                .AddPolicy(Politicas.CidadaoOuGestor, politica => politica.RequireRole(Papel.Cidadao, Papel.Gestor));

            return services;
        }

        private static async Task VincularUsuarioLocalAsync(TokenValidatedContext context, JwtSettings settings)
        {
            var principal = context.Principal;
            if (principal?.Identity is not ClaimsIdentity identidade)
            {
                return;
            }

            var idUsuarioLocal = principal.FindFirst(ClaimsConfere.Sub)?.Value;
            var emissor = principal.FindFirst(ClaimsConfere.Emissor)?.Value;

            if (emissor == settings.IssuerJava && principal.IsInRole(Papel.Cidadao))
            {
                var email = principal.FindFirst(ClaimsConfere.Email)?.Value;
                if (string.IsNullOrWhiteSpace(email))
                {
                    context.Fail("O token da API Java não tem o e-mail do cidadão.");
                    return;
                }

                var nome = principal.FindFirst(ClaimsConfere.Nome)?.Value ?? email;
                var codIbge = int.TryParse(principal.FindFirst(ClaimsConfere.Municipio)?.Value, out var codigo) ? codigo : (int?)null;

                var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
                var idLocal = await authService.ResolverCidadaoDaApiJavaAsync(email, nome, codIbge);
                idUsuarioLocal = idLocal.ToString();
            }

            if (idUsuarioLocal is not null)
            {
                identidade.AddClaim(new Claim(ClaimsConfere.IdUsuarioLocal, idUsuarioLocal));
            }
        }

        private static async Task EscreverProblemaAsync(HttpContext httpContext, int status, string codigo, string titulo, string detalhe)
        {
            httpContext.Response.StatusCode = status;

            var problema = new ProblemDetails
            {
                Status = status,
                Title = titulo,
                Detail = detalhe,
                Instance = httpContext.Request.Path
            };

            problema.Extensions["codigo"] = codigo;

            var problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problema
            });
        }
    }
}
