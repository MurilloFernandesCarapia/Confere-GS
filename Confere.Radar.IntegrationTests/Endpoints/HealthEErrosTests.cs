using System.Net;
using Confere.Radar.IntegrationTests.Infra;

namespace Confere.Radar.IntegrationTests.Endpoints
{
    public class HealthEErrosTests : IClassFixture<ConfereApiFactory>
    {
        private readonly HttpClient _cliente;

        public HealthEErrosTests(ConfereApiFactory factory)
        {
            _cliente = factory.CreateClient();
        }

        [Fact]
        public async Task HealthLive_Devolve200Healthy()
        {
            var resposta = await _cliente.EnviarAsync(HttpMethod.Get, "/health/live");

            Assert.Equal(HttpStatusCode.OK, resposta.Status);
            Assert.Equal("Healthy", resposta.Corpo.GetProperty("status").GetString());
        }

        [Fact]
        public async Task HealthReady_MostraBancoEServicosExternos()
        {
            var resposta = await _cliente.EnviarAsync(HttpMethod.Get, "/health/ready");
            var componentes = resposta.Corpo.GetProperty("componentes").EnumerateArray()
                .ToDictionary(c => c.GetProperty("nome").GetString()!, c => c.GetProperty("status").GetString());

            Assert.Equal(HttpStatusCode.OK, resposta.Status);
            Assert.Equal("Degraded", resposta.Corpo.GetProperty("status").GetString());
            Assert.Equal("Healthy", componentes["banco"]);
            Assert.Equal("Degraded", componentes["api-java"]);
            Assert.Equal("Degraded", componentes["servico-ia"]);
        }

        [Fact]
        public async Task RotaInexistente_Devolve404NoPadraoDeErro()
        {
            var resposta = await _cliente.EnviarAsync(HttpMethod.Get, "/rota-que-nao-existe");

            Assert.Equal(HttpStatusCode.NotFound, resposta.Status);
            Assert.Equal("NAO_ENCONTRADO", resposta.Codigo);
        }

        [Fact]
        public async Task MetodoNaoPermitido_Devolve405NoPadraoDeErro()
        {
            var resposta = await _cliente.EnviarAsync(HttpMethod.Get, "/denuncias");

            Assert.Equal(HttpStatusCode.MethodNotAllowed, resposta.Status);
            Assert.Equal("METODO_NAO_PERMITIDO", resposta.Codigo);
        }

        [Fact]
        public async Task JsonMalFormado_Devolve400()
        {
            var token = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());
            using var requisicao = new HttpRequestMessage(HttpMethod.Put, "/perfil-social")
            {
                Content = new StringContent("{ isso não é json", System.Text.Encoding.UTF8, "application/json")
            };
            requisicao.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var resposta = await _cliente.SendAsync(requisicao);

            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        }

        [Fact]
        public async Task Swagger_DocumentaOHeaderDeIdempotencia()
        {
            var json = await _cliente.GetStringAsync("/swagger/v1/swagger.json");

            Assert.Contains("\"Idempotency-Key\"", json);
            Assert.Contains("/elegibilidades/avaliar", json);
        }
    }
}
