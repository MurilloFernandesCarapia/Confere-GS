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

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IBeneficioService, BeneficioService>();

            return services;
        }
    }
}
