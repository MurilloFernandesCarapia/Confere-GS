using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Infrastructure.Data;
using Confere.Radar.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Confere.Radar.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Default")));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
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

            return services;
        }
    }
}
