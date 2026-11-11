using System.Text;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Infrastructure.Data;
using Confere.Radar.Infrastructure.HealthChecks;
using Confere.Radar.Infrastructure.Repositories;
using Confere.Radar.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Confere.Radar.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Default")));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IMunicipioRepository, MunicipioRepository>();
            services.AddScoped<ITipoGolpeRepository, TipoGolpeRepository>();
            services.AddScoped<IGestorRepository, GestorRepository>();
            services.AddScoped<IIdentificadorRepository, IdentificadorRepository>();
            services.AddScoped<IDenunciaRepository, DenunciaRepository>();
            services.AddScoped<IAlertaRegionalRepository, AlertaRegionalRepository>();
            services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();
            services.AddScoped<IBeneficioRepository, BeneficioRepository>();
            services.AddScoped<IPerfilSocialRepository, PerfilSocialRepository>();
            services.AddScoped<IElegibilidadeRepository, ElegibilidadeRepository>();

            services.AddOptions<JwtSettings>()
                .Bind(configuration.GetSection(JwtSettings.Secao))
                .Validate(s => Encoding.UTF8.GetByteCount(s.Secret) >= 32,
                    "Configure Jwt:Secret com pelo menos 32 caracteres (User Secrets ou variável de ambiente Jwt__Secret).")
                .ValidateOnStart();

            services.AddSingleton<ISenhaHasher, BCryptSenhaHasher>();
            services.AddSingleton<ITokenService, JwtTokenService>();

            services.AddHealthChecksConfere();

            return services;
        }

        private static void AddHealthChecksConfere(this IServiceCollection services)
        {
            services.AddHttpClient(ServicoExternoHealthCheck.NomeClienteHttp, cliente =>
            {
                cliente.Timeout = TimeSpan.FromSeconds(3);
            });

            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy("A API está no ar."), tags: new[] { "live" })
                .AddDbContextCheck<AppDbContext>("banco", tags: new[] { "ready" })
                .AddTypeActivatedCheck<ServicoExternoHealthCheck>("api-java", HealthStatus.Degraded, new[] { "ready" }, "Services:JavaHealthUrl")
                .AddTypeActivatedCheck<ServicoExternoHealthCheck>("servico-ia", HealthStatus.Degraded, new[] { "ready" }, "Services:IaHealthUrl");
        }
    }
}
