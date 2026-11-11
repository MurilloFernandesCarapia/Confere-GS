using System.Net;
using Confere.Radar.IntegrationTests.Infra;

namespace Confere.Radar.IntegrationTests.Endpoints
{
    public class DireitosEndpointsTests : IClassFixture<ConfereApiFactory>
    {
        private readonly HttpClient _cliente;

        public DireitosEndpointsTests(ConfereApiFactory factory)
        {
            _cliente = factory.CreateClient();
        }

        private static object PerfilDoContrato(bool consentimento = true) => new
        {
            idade = 68,
            rendaPcFaixa = "ATE_1_4",
            qtdMoradores = 2,
            inscritoCadunico = true,
            recebeBpc = false,
            deficiencia = false,
            vinculoFormal = false,
            codIbgeMunicipio = 3550308,
            consentimento
        };

        [Fact]
        public async Task Beneficios_ListaCatalogoEDetalhe404ParaCodigoInexistente()
        {
            var token = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());

            var lista = await _cliente.EnviarAsync(HttpMethod.Get, "/beneficios", token);
            var detalhe = await _cliente.EnviarAsync(HttpMethod.Get, "/beneficios/bpc_idoso", token);
            var inexistente = await _cliente.EnviarAsync(HttpMethod.Get, "/beneficios/XYZ", token);

            Assert.Equal(5, lista.Corpo.GetArrayLength());
            Assert.Equal(3, detalhe.Corpo.GetProperty("passos").GetArrayLength());
            Assert.Equal(HttpStatusCode.NotFound, inexistente.Status);
        }

        [Fact]
        public async Task RN17_PerfilSemConsentimento_Devolve403ConsentimentoPendente()
        {
            var token = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());

            var resposta = await _cliente.EnviarAsync(HttpMethod.Put, "/perfil-social", token, PerfilDoContrato(consentimento: false));

            Assert.Equal(HttpStatusCode.Forbidden, resposta.Status);
            Assert.Equal("CONSENTIMENTO_PENDENTE", resposta.Codigo);
        }

        [Fact]
        public async Task RN17_RendaComValorExato_Devolve400()
        {
            var token = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());

            var resposta = await _cliente.EnviarAsync(HttpMethod.Put, "/perfil-social", token, new { rendaPcFaixa = "1518.00", consentimento = true });

            Assert.Equal(HttpStatusCode.BadRequest, resposta.Status);
        }

        [Fact]
        public async Task RN16_AvaliarSemPerfil_Devolve403ConsentimentoPendente()
        {
            var token = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());

            var resposta = await _cliente.EnviarAsync(HttpMethod.Post, "/elegibilidades/avaliar", token, idempotencyKey: ClienteApi.NovaChave());

            Assert.Equal(HttpStatusCode.Forbidden, resposta.Status);
            Assert.Equal("CONSENTIMENTO_PENDENTE", resposta.Codigo);
        }

        [Fact]
        public async Task RN16_RN18_FluxoCompletoDeDireitos()
        {
            var token = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());
            var chave = ClienteApi.NovaChave();

            var perfil = await _cliente.EnviarAsync(HttpMethod.Put, "/perfil-social", token, PerfilDoContrato());
            var avaliacao = await _cliente.EnviarAsync(HttpMethod.Post, "/elegibilidades/avaliar", token, idempotencyKey: chave);
            var replay = await _cliente.EnviarAsync(HttpMethod.Post, "/elegibilidades/avaliar", token, idempotencyKey: chave);

            Assert.Equal(HttpStatusCode.OK, perfil.Status);
            Assert.Equal(HttpStatusCode.OK, avaliacao.Status);
            Assert.Equal("true", replay.Header("Idempotent-Replayed"));

            var resultados = avaliacao.Corpo.EnumerateArray().ToDictionary(
                e => e.GetProperty("beneficio").GetProperty("codigo").GetString()!,
                e => e);
            Assert.Equal("PROVAVEL", resultados["TARIFA_SOCIAL"].GetProperty("status").GetString());
            Assert.Equal("PROVAVEL", resultados["BPC_IDOSO"].GetProperty("status").GetString());
            Assert.Equal("INELEGIVEL", resultados["BPC_PCD"].GetProperty("status").GetString());
            Assert.Equal("POSSIVEL", resultados["VALORES_A_RECEBER"].GetProperty("status").GetString());

            var idTarifa = resultados["TARIFA_SOCIAL"].GetProperty("id").GetInt32();
            var solicitado = await _cliente.EnviarAsync(HttpMethod.Patch, $"/elegibilidades/{idTarifa}", token, new { status = "SOLICITADO" });
            await _cliente.EnviarAsync(HttpMethod.Put, "/perfil-social", token, new { rendaPcFaixa = "ACIMA_2", consentimento = true });
            var reavaliacao = await _cliente.EnviarAsync(HttpMethod.Post, "/elegibilidades/avaliar", token, idempotencyKey: ClienteApi.NovaChave());
            var tarifaDepois = reavaliacao.Corpo.EnumerateArray().Single(e => e.GetProperty("id").GetInt32() == idTarifa);

            Assert.Equal(HttpStatusCode.OK, solicitado.Status);
            Assert.Equal("SOLICITADO", tarifaDepois.GetProperty("status").GetString());
        }

        [Fact]
        public async Task ElegibilidadeDeOutroCidadao_Devolve403()
        {
            var dono = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());
            var outro = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());
            await _cliente.EnviarAsync(HttpMethod.Put, "/perfil-social", dono, PerfilDoContrato());
            var avaliacao = await _cliente.EnviarAsync(HttpMethod.Post, "/elegibilidades/avaliar", dono, idempotencyKey: ClienteApi.NovaChave());
            var id = avaliacao.Corpo[0].GetProperty("id").GetInt32();

            var resposta = await _cliente.EnviarAsync(HttpMethod.Patch, $"/elegibilidades/{id}", outro, new { status = "SOLICITADO" });

            Assert.Equal(HttpStatusCode.Forbidden, resposta.Status);
        }

        [Fact]
        public async Task ExcluirPerfil_Devolve204EDepois404()
        {
            var token = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());
            await _cliente.EnviarAsync(HttpMethod.Put, "/perfil-social", token, PerfilDoContrato());

            var exclusao = await _cliente.EnviarAsync(HttpMethod.Delete, "/perfil-social", token);
            var depois = await _cliente.EnviarAsync(HttpMethod.Get, "/perfil-social", token);

            Assert.Equal(HttpStatusCode.NoContent, exclusao.Status);
            Assert.Equal(HttpStatusCode.NotFound, depois.Status);
        }
    }
}
