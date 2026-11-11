using System.Net;
using Confere.Radar.IntegrationTests.Infra;

namespace Confere.Radar.IntegrationTests.Endpoints
{
    public class RateLimitingTests : IClassFixture<ConfereApiFactoryComLimiteBaixo>
    {
        private readonly HttpClient _cliente;

        public RateLimitingTests(ConfereApiFactoryComLimiteBaixo factory)
        {
            _cliente = factory.CreateClient();
        }

        [Fact]
        public async Task ConsultaPublica_PassandoDoLimite_Devolve429ComRetryAfter()
        {
            for (var i = 0; i < ConfereApiFactoryComLimiteBaixo.Consultas; i++)
            {
                var permitida = await _cliente.EnviarAsync(HttpMethod.Get, "/radar/consulta?tipo=TELEFONE&valor=11912345678");
                Assert.Equal(HttpStatusCode.OK, permitida.Status);
            }

            var bloqueada = await _cliente.EnviarAsync(HttpMethod.Get, "/radar/consulta?tipo=TELEFONE&valor=11912345678");

            Assert.Equal(HttpStatusCode.TooManyRequests, bloqueada.Status);
            Assert.Equal("LIMITE_EXCEDIDO", bloqueada.Codigo);
            Assert.NotNull(bloqueada.Mensagem.Headers.RetryAfter);
            Assert.True(bloqueada.Corpo.GetProperty("tentarNovamenteEmSegundos").GetInt32() > 0);
        }

        [Fact]
        public async Task Denuncias_PassandoDoLimiteDoUsuario_Devolve429SoParaEle()
        {
            var cidadao = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());
            var outroCidadao = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());

            for (var i = 0; i < ConfereApiFactoryComLimiteBaixo.Denuncias; i++)
            {
                var permitida = await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", cidadao, ClienteApi.NovaDenuncia(ClienteApi.NovoTelefone()), ClienteApi.NovaChave());
                Assert.Equal(HttpStatusCode.Created, permitida.Status);
            }

            var bloqueada = await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", cidadao, ClienteApi.NovaDenuncia(ClienteApi.NovoTelefone()), ClienteApi.NovaChave());
            var deOutraPessoa = await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", outroCidadao, ClienteApi.NovaDenuncia(ClienteApi.NovoTelefone()), ClienteApi.NovaChave());

            Assert.Equal(HttpStatusCode.TooManyRequests, bloqueada.Status);
            Assert.Equal(HttpStatusCode.Created, deOutraPessoa.Status);
        }

        [Fact]
        public async Task LimiteGlobal_PorUsuario_Devolve429DepoisDoLimite()
        {
            var cidadao = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());
            var status = new List<HttpStatusCode>();

            for (var i = 0; i <= ConfereApiFactoryComLimiteBaixo.Global; i++)
            {
                status.Add((await _cliente.EnviarAsync(HttpMethod.Get, "/beneficios", cidadao)).Status);
            }

            Assert.All(status.Take(ConfereApiFactoryComLimiteBaixo.Global), s => Assert.Equal(HttpStatusCode.OK, s));
            Assert.Equal(HttpStatusCode.TooManyRequests, status[^1]);
        }

        [Fact]
        public async Task HealthCheck_NaoEntraNoLimite()
        {
            for (var i = 0; i < ConfereApiFactoryComLimiteBaixo.Global + 5; i++)
            {
                var resposta = await _cliente.GetAsync("/health/live");
                Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
            }
        }
    }
}
