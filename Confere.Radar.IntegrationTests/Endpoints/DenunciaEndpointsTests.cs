using System.Net;
using Confere.Radar.IntegrationTests.Infra;

namespace Confere.Radar.IntegrationTests.Endpoints
{
    public class DenunciaEndpointsTests : IClassFixture<ConfereApiFactory>
    {
        private readonly HttpClient _cliente;

        public DenunciaEndpointsTests(ConfereApiFactory factory)
        {
            _cliente = factory.CreateClient();
        }

        [Fact]
        public async Task CriarDenuncia_SemToken_Devolve401()
        {
            var resposta = await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", corpo: ClienteApi.NovaDenuncia(ClienteApi.NovoTelefone()), idempotencyKey: ClienteApi.NovaChave());

            Assert.Equal(HttpStatusCode.Unauthorized, resposta.Status);
        }

        [Fact]
        public async Task CriarDenuncia_ComTokenDeGestor_Devolve403()
        {
            var gestor = await _cliente.LoginGestorSeedAsync();

            var resposta = await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", gestor, ClienteApi.NovaDenuncia(ClienteApi.NovoTelefone()), ClienteApi.NovaChave());

            Assert.Equal(HttpStatusCode.Forbidden, resposta.Status);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("nao-e-um-uuid")]
        public async Task RN18_SemIdempotencyKeyValida_Devolve400(string? chave)
        {
            var token = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());

            var resposta = await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", token, ClienteApi.NovaDenuncia(ClienteApi.NovoTelefone()), chave);

            Assert.Equal(HttpStatusCode.BadRequest, resposta.Status);
            Assert.Equal("Idempotency-Key", resposta.Corpo.GetProperty("erros")[0].GetProperty("campo").GetString());
        }

        [Fact]
        public async Task RN18_MesmaChaveMesmoCorpo_Devolve201ERepeteARespostaSemDuplicar()
        {
            var token = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());
            var telefone = ClienteApi.NovoTelefone();
            var chave = ClienteApi.NovaChave();

            var primeira = await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", token, ClienteApi.NovaDenuncia(telefone), chave);
            var replay = await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", token, ClienteApi.NovaDenuncia(telefone), chave);
            var consulta = await _cliente.EnviarAsync(HttpMethod.Get, $"/radar/consulta?tipo=TELEFONE&valor={telefone}");

            Assert.Equal(HttpStatusCode.Created, primeira.Status);
            Assert.Equal($"/denuncias/{primeira.Corpo.GetProperty("id").GetInt32()}", primeira.Mensagem.Headers.Location?.ToString());
            Assert.Null(primeira.Header("Idempotent-Replayed"));
            Assert.Equal(HttpStatusCode.Created, replay.Status);
            Assert.Equal("true", replay.Header("Idempotent-Replayed"));
            Assert.Equal(primeira.Corpo.GetProperty("id").GetInt32(), replay.Corpo.GetProperty("id").GetInt32());
            Assert.Equal(1, consulta.Corpo.GetProperty("totalDenuncias").GetInt32());
        }

        [Fact]
        public async Task RN18_MesmaChaveCorpoDiferente_Devolve422()
        {
            var token = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());
            var telefone = ClienteApi.NovoTelefone();
            var chave = ClienteApi.NovaChave();

            await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", token, ClienteApi.NovaDenuncia(telefone), chave);
            var resposta = await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", token, ClienteApi.NovaDenuncia(telefone, "FALSO_PARENTE"), chave);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.Status);
            Assert.Equal("IDEMPOTENCIA_CORPO_DIFERENTE", resposta.Codigo);
        }

        [Fact]
        public async Task RN12_DenunciaRepetidaEm24h_Devolve409()
        {
            var token = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());
            var telefone = ClienteApi.NovoTelefone();

            await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", token, ClienteApi.NovaDenuncia(telefone), ClienteApi.NovaChave());
            var resposta = await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", token, ClienteApi.NovaDenuncia(telefone), ClienteApi.NovaChave());

            Assert.Equal(HttpStatusCode.Conflict, resposta.Status);
            Assert.Equal("DENUNCIA_DUPLICADA", resposta.Codigo);
        }

        [Fact]
        public async Task CriarDenuncia_DadosInvalidos_Devolve400ComErros()
        {
            var token = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());

            var resposta = await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", token,
                new { tipo = "FAX", valor = "", tipoGolpeCodigo = "", codIbgeMunicipio = 1 }, ClienteApi.NovaChave());

            Assert.Equal(HttpStatusCode.BadRequest, resposta.Status);
            Assert.Equal(4, resposta.Corpo.GetProperty("erros").GetArrayLength());
        }

        [Fact]
        public async Task DenunciaDeOutroCidadao_Devolve403EInexistente404()
        {
            var dono = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());
            var outro = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());
            var criada = await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", dono, ClienteApi.NovaDenuncia(ClienteApi.NovoTelefone()), ClienteApi.NovaChave());
            var id = criada.Corpo.GetProperty("id").GetInt32();

            var deOutro = await _cliente.EnviarAsync(HttpMethod.Get, $"/denuncias/{id}", outro);
            var doDono = await _cliente.EnviarAsync(HttpMethod.Get, $"/denuncias/{id}", dono);
            var inexistente = await _cliente.EnviarAsync(HttpMethod.Get, "/denuncias/999999", dono);

            Assert.Equal(HttpStatusCode.Forbidden, deOutro.Status);
            Assert.Equal(HttpStatusCode.OK, doDono.Status);
            Assert.Equal(HttpStatusCode.NotFound, inexistente.Status);
        }

        [Fact]
        public async Task EditarEExcluir_AtualizaERecalculaOIdentificador()
        {
            var token = TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail());
            var telefone = ClienteApi.NovoTelefone();
            var criada = await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", token, ClienteApi.NovaDenuncia(telefone), ClienteApi.NovaChave());
            var id = criada.Corpo.GetProperty("id").GetInt32();

            var editada = await _cliente.EnviarAsync(HttpMethod.Put, $"/denuncias/{id}", token, new { tipoGolpeCodigo = "FALSO_PARENTE", descricao = "Era o golpe do falso parente" });
            var minhas = await _cliente.EnviarAsync(HttpMethod.Get, "/denuncias/minhas?pagina=0&tamanho=5", token);
            var excluida = await _cliente.EnviarAsync(HttpMethod.Delete, $"/denuncias/{id}", token);
            var consulta = await _cliente.EnviarAsync(HttpMethod.Get, $"/radar/consulta?tipo=TELEFONE&valor={telefone}");

            Assert.Equal("FALSO_PARENTE", editada.Corpo.GetProperty("tipoGolpe").GetProperty("codigo").GetString());
            Assert.Equal(1, minhas.Corpo.GetProperty("totalItens").GetInt32());
            Assert.Equal(HttpStatusCode.NoContent, excluida.Status);
            Assert.False(consulta.Corpo.GetProperty("encontrado").GetBoolean());
        }

        [Fact]
        public async Task RN13_RN14_TresNumerosConfirmadosNoMunicipio_CriamAlertaAutomatico()
        {
            const int guarulhos = 3518800;
            var cidadaos = Enumerable.Range(0, 5).Select(_ => TokenTeste.CidadaoDaApiJava(ClienteApi.NovoEmail(), guarulhos)).ToList();
            var telefones = Enumerable.Range(0, 3).Select(_ => ClienteApi.NovoTelefone()).ToList();
            RespostaApi? ultima = null;

            foreach (var telefone in telefones)
            {
                foreach (var cidadao in cidadaos)
                {
                    ultima = await _cliente.EnviarAsync(HttpMethod.Post, "/denuncias", cidadao, ClienteApi.NovaDenuncia(telefone, "FALSO_BOLETO", guarulhos), ClienteApi.NovaChave());
                    Assert.Equal(HttpStatusCode.Created, ultima.Status);
                }
            }

            var identificador = ultima!.Corpo.GetProperty("identificador");
            Assert.Equal(5, identificador.GetProperty("totalDenuncias").GetInt32());
            Assert.Equal(80, identificador.GetProperty("score").GetInt32());
            Assert.Equal("CONFIRMADO", identificador.GetProperty("status").GetString());
            Assert.True(ultima.Corpo.GetProperty("alertaRegionalCriado").GetBoolean());

            var alertas = await _cliente.EnviarAsync(HttpMethod.Get, "/alertas-regionais", cidadaos[0]);
            var automatico = alertas.Corpo.EnumerateArray().Single(a => a.GetProperty("tipoGolpe").GetProperty("codigo").GetString() == "FALSO_BOLETO");
            Assert.Equal("AUTOMATICO", automatico.GetProperty("origem").GetString());
            Assert.Equal("ATENCAO", automatico.GetProperty("nivel").GetString());
        }
    }
}
