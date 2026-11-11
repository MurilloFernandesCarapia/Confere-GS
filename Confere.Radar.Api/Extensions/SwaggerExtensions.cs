using Microsoft.OpenApi;

namespace Confere.Radar.Api.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerConfere(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Confere Radar API",
                    Version = "v1",
                    Description = "API .NET do Confere (Global Solution FIAP 2026 - CitizenTech): radar comunitário de golpes, Meus direitos e painel do gestor público. " +
                                  "Gestores fazem login em POST /auth/token. Cidadãos usam o token emitido pela API Java. Clique em Authorize e cole só o token, sem a palavra Bearer."
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Cole aqui apenas o token JWT, sem a palavra Bearer."
                });

                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            });

            return services;
        }

        public static WebApplication UseSwaggerConfere(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Confere Radar API v1");
                c.DocumentTitle = "Confere Radar API";
            });

            return app;
        }
    }
}
