using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Confere.Radar.Infrastructure.HealthChecks
{
    public class ServicoExternoHealthCheck : IHealthCheck
    {
        public const string NomeClienteHttp = "health-servicos-externos";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _chaveUrl;

        public ServicoExternoHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration, string chaveUrl)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _chaveUrl = chaveUrl;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var url = _configuration[_chaveUrl];
            if (string.IsNullOrWhiteSpace(url))
            {
                return new HealthCheckResult(context.Registration.FailureStatus, $"URL não configurada ({_chaveUrl}).");
            }

            try
            {
                var cliente = _httpClientFactory.CreateClient(NomeClienteHttp);
                using var resposta = await cliente.GetAsync(url, cancellationToken);

                if (resposta.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Healthy($"Respondeu {(int)resposta.StatusCode}.");
                }

                return new HealthCheckResult(context.Registration.FailureStatus, $"Respondeu {(int)resposta.StatusCode}.");
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, "Serviço fora do ar ou sem resposta.", ex);
            }
        }
    }
}
