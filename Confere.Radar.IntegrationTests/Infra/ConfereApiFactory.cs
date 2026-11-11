using Confere.Radar.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Confere.Radar.IntegrationTests.Infra
{
    public class ConfereApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _nomeBanco = $"confere-testes-{Guid.NewGuid()}";

        protected virtual int LimiteConsultaPublica => 10000;
        protected virtual int LimiteDenuncias => 10000;
        protected virtual int LimiteGlobal => 10000;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.UseSetting("ConnectionStrings:Default", "Server=nao-usado-nos-testes");
            builder.UseSetting("Jwt:Secret", TokenTeste.Segredo);
            builder.UseSetting("Services:JavaHealthUrl", string.Empty);
            builder.UseSetting("Services:IaHealthUrl", string.Empty);
            builder.UseSetting("RateLimiting:ConsultaPublica:Limite", LimiteConsultaPublica.ToString());
            builder.UseSetting("RateLimiting:Denuncia:Tokens", LimiteDenuncias.ToString());
            builder.UseSetting("RateLimiting:Denuncia:TokensPorPeriodo", LimiteDenuncias.ToString());
            builder.UseSetting("RateLimiting:Global:Limite", LimiteGlobal.ToString());

            builder.ConfigureTestServices(services =>
            {
                var registrosDoBanco = services
                    .Where(s => s.ServiceType == typeof(DbContextOptions<AppDbContext>)
                             || s.ServiceType == typeof(DbContextOptions)
                             || (s.ServiceType.IsGenericType && s.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration")))
                    .ToList();

                foreach (var registro in registrosDoBanco)
                {
                    services.Remove(registro);
                }

                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_nomeBanco));
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            using var scope = host.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();

            return host;
        }
    }

    public class ConfereApiFactoryComLimiteBaixo : ConfereApiFactory
    {
        public const int Consultas = 3;
        public const int Denuncias = 2;
        public const int Global = 30;

        protected override int LimiteConsultaPublica => Consultas;
        protected override int LimiteDenuncias => Denuncias;
        protected override int LimiteGlobal => Global;
    }
}
