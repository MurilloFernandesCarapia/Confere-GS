using Confere.Radar.Application.Services;
using Confere.Radar.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Confere.Radar.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton(TimeProvider.System);
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            services.AddSingleton<INormalizadorIdentificador, NormalizadorIdentificador>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IBeneficioService, BeneficioService>();
            services.AddScoped<IRadarScoreService, RadarScoreService>();
            services.AddScoped<IAlertaRegionalService, AlertaRegionalService>();
            services.AddScoped<IDenunciaService, DenunciaService>();
            services.AddScoped<IRadarService, RadarService>();
            services.AddScoped<IGestaoService, GestaoService>();
            services.AddScoped<IPerfilSocialService, PerfilSocialService>();
            services.AddScoped<IElegibilidadeService, ElegibilidadeService>();

            return services;
        }
    }
}
