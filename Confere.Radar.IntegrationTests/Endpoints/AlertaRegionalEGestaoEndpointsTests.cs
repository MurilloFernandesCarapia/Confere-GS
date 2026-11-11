using System.Net;
using Confere.Radar.IntegrationTests.Infra;

namespace Confere.Radar.IntegrationTests.Endpoints
{
    public class AlertaRegionalEGestaoEndpointsTests : IClassFixture<ConfereApiFactory>
    {
        private readonly HttpClient _cliente;

        public AlertaRegionalEGestaoEndpointsTests(ConfereApiFactory factory)
        {
            _cliente = factory.CreateClient();
        }

        private static object NovoAlerta(int codIbge = 3550308) => new
        {
            codIbgeMunicipio = codIbge,
            tipoGolpeCodigo = "FALSA_PROVA_VIDA",
            titulo = "Golpe da prova de vida",
            mensagem = "O INSS não pede selfie por WhatsApp.",
            nivel = "URGENTE",
            dtFim = DateTimeOffset.UtcNow.AddDays(7)
        };

        [Fact]
        public async Task CriarAlerta_ComTokenDeCidadao_Devolve403()
        {
            var cidadao = await _cliente.LoginCidadaoSeedAsync();

            var resposta = await _cliente.EnviarAsync(HttpMethod.Post, "/alertas-regionais", cidadao, NovoAlerta());

            Assert.Equal(HttpStatusCode.Forbidden, resposta.Status);
        }

        [Fact]
        public async Task CicloDoAlerta_GestorCriaEditaEEncerra()
        {
            var gestor = await _cliente.LoginGestorSeedAsync();
            var cidadao = await _cliente.LoginCidadaoSeedAsync();

            var criado = await _cliente.EnviarAsync(HttpMethod.Post, "/alertas-regionais", gestor, NovoAlerta());
            var id = criado.Corpo.GetProperty("id").GetInt32();
            var listaDoCidadao = await _cliente.EnviarAsync(HttpMethod.Get, "/alertas-regionais", cidadao);
            var editado = await _cliente.EnviarAsync(HttpMethod.Put, $"/alertas-regionais/{id}", gestor, new
            {
                tipoGolpeCodigo = "FALSA_PROVA_VIDA",
                titulo = "Prova de vida: atenção",
                mensagem = "Nunca mande selfie para quem ligar dizendo ser do INSS.",
                nivel = "ATENCAO",
                dtFim = DateTimeOffset.UtcNow.AddDays(10)
            });
            var encerrado = await _cliente.EnviarAsync(HttpMethod.Delete, $"/alertas-regionais/{id}", gestor);
            var listaDepois = await _cliente.EnviarAsync(HttpMethod.Get, "/alertas-regionais?codIbge=3550308", gestor);

            Assert.Equal(HttpStatusCode.Created, criado.Status);
            Assert.Equal("GESTOR", criado.Corpo.GetProperty("origem").GetString());
            Assert.Contains(listaDoCidadao.Corpo.EnumerateArray(), a => a.GetProperty("id").GetInt32() == id);
            Assert.Equal("ATENCAO", editado.Corpo.GetProperty("nivel").GetString());
            Assert.Equal(HttpStatusCode.NoContent, encerrado.Status);
            Assert.DoesNotContain(listaDepois.Corpo.EnumerateArray(), a => a.GetProperty("id").GetInt32() == id);
        }

        [Fact]
        public async Task CriarAlerta_EmOutroMunicipio_Devolve403()
        {
            var gestor = await _cliente.LoginGestorSeedAsync();

            var resposta = await _cliente.EnviarAsync(HttpMethod.Post, "/alertas-regionais", gestor, NovoAlerta(3509502));

            Assert.Equal(HttpStatusCode.Forbidden, resposta.Status);
            Assert.Equal("SEM_PERMISSAO", resposta.Codigo);
        }

        [Fact]
        public async Task ListarAlertas_MunicipioInexistente_Devolve404()
        {
            var gestor = await _cliente.LoginGestorSeedAsync();

            var resposta = await _cliente.EnviarAsync(HttpMethod.Get, "/alertas-regionais?codIbge=1234567", gestor);

            Assert.Equal(HttpStatusCode.NotFound, resposta.Status);
        }

        [Fact]
        public async Task Gestao_ComTokenDeCidadao_Devolve403()
        {
            var cidadao = await _cliente.LoginCidadaoSeedAsync();

            var resposta = await _cliente.EnviarAsync(HttpMethod.Get, "/gestao/identificadores", cidadao);

            Assert.Equal(HttpStatusCode.Forbidden, resposta.Status);
        }

        [Fact]
        public async Task RN13_GestorDescartaIdentificador_QueNaoVoltaAConfirmadoSozinho()
        {
            var gestor = await _cliente.LoginGestorSeedAsync();
            var telefone = ClienteApi.NovoTelefone();
            var criada = await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail()), ClienteApi.NovaDenuncia(telefone), ClienteApi.NovaChave());
            var idIdentificador = criada.Corpo.GetProperty("identificador").GetProperty("id").GetInt32();

            var descartado = await _cliente.EnviarAsync(HttpMethod.Patch, $"/gestao/identificadores/{idIdentificador}/status", gestor, new { status = "DESCARTADO" });
            var novaDenuncia = await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail()), ClienteApi.NovaDenuncia(telefone), ClienteApi.NovaChave());
            var lista = await _cliente.EnviarAsync(HttpMethod.Get, "/gestao/identificadores?status=DESCARTADO", gestor);

            Assert.Equal("DESCARTADO", descartado.Corpo.GetProperty("status").GetString());
            Assert.Equal("DESCARTADO", novaDenuncia.Corpo.GetProperty("identificador").GetProperty("status").GetString());
            Assert.Contains(lista.Corpo.GetProperty("conteudo").EnumerateArray(), i => i.GetProperty("id").GetInt32() == idIdentificador);
        }

        [Fact]
        public async Task AtualizarStatusIdentificador_ParaConfirmado_Devolve400()
        {
            var gestor = await _cliente.LoginGestorSeedAsync();

            var resposta = await _cliente.EnviarAsync(HttpMethod.Patch, "/gestao/identificadores/1/status", gestor, new { status = "CONFIRMADO" });

            Assert.Equal(HttpStatusCode.BadRequest, resposta.Status);
        }

        [Fact]
        public async Task Painel_DoMunicipioDoGestor_Devolve200EDeOutro403()
        {
            var gestor = await _cliente.LoginGestorSeedAsync();

            var painel = await _cliente.EnviarAsync(HttpMethod.Get, "/gestao/painel", gestor);
            var outro = await _cliente.EnviarAsync(HttpMethod.Get, "/gestao/painel?codIbge=3509502", gestor);
            var dataInvalida = await _cliente.EnviarAsync(HttpMethod.Get, "/gestao/painel?de=abc", gestor);

            Assert.Equal(HttpStatusCode.OK, painel.Status);
            Assert.Equal(3550308, painel.Corpo.GetProperty("municipio").GetProperty("codIbge").GetInt32());
            Assert.Equal(HttpStatusCode.Forbidden, outro.Status);
            Assert.Equal(HttpStatusCode.BadRequest, dataInvalida.Status);
        }
    }
}
