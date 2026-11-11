using System.Net;
using Confere.Radar.IntegrationTests.Infra;

namespace Confere.Radar.IntegrationTests.Endpoints
{
    public class RadarEndpointsTests : IClassFixture<ConfereApiFactory>
    {
        private readonly HttpClient _cliente;

        public RadarEndpointsTests(ConfereApiFactory factory)
        {
            _cliente = factory.CreateClient();
        }

        [Fact]
        public async Task RN15_ConsultaPublica_SemLogin_NaoMostraQuemDenunciou()
        {
            var telefone = ClienteApi.NovoTelefone();
            await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail()), ClienteApi.NovaDenuncia(telefone), ClienteApi.NovaChave());

            var resposta = await _cliente.EnviarAsync(HttpMethod.Get, $"/radar/consulta?tipo=TELEFONE&valor={telefone}");
            var propriedades = resposta.Corpo.EnumerateObject().Select(p => p.Name).ToList();

            Assert.Equal(HttpStatusCode.OK, resposta.Status);
            Assert.True(resposta.Corpo.GetProperty("encontrado").GetBoolean());
            Assert.Equal("Falsa central do banco", resposta.Corpo.GetProperty("tipoMaisComum").GetString());
            Assert.Equal(new[] { "encontrado", "status", "totalDenuncias", "tipoMaisComum", "ultimaDenuncia" }, propriedades);
        }

        [Fact]
        public async Task ConsultaPublica_NumeroNuncaDenunciado_DevolveNaoEncontrado()
        {
            var resposta = await _cliente.EnviarAsync(HttpMethod.Get, $"/radar/consulta?tipo=TELEFONE&valor={ClienteApi.NovoTelefone()}");

            Assert.Equal(HttpStatusCode.OK, resposta.Status);
            Assert.False(resposta.Corpo.GetProperty("encontrado").GetBoolean());
        }

        [Fact]
        public async Task ConsultaPublica_TipoInvalido_Devolve400()
        {
            var resposta = await _cliente.EnviarAsync(HttpMethod.Get, "/radar/consulta?tipo=FAX&valor=123");

            Assert.Equal(HttpStatusCode.BadRequest, resposta.Status);
            Assert.Equal("tipo", resposta.Corpo.GetProperty("erros")[0].GetProperty("campo").GetString());
        }

        [Fact]
        public async Task TiposDeGolpe_SemLogin_DevolveOCatalogo()
        {
            var resposta = await _cliente.EnviarAsync(HttpMethod.Get, "/radar/tipos-golpe");

            Assert.Equal(HttpStatusCode.OK, resposta.Status);
            Assert.Equal(10, resposta.Corpo.GetArrayLength());
        }
    }
}
