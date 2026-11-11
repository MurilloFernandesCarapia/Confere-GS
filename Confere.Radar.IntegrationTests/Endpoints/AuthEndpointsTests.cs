using System.Net;
using Confere.Radar.IntegrationTests.Infra;

namespace Confere.Radar.IntegrationTests.Endpoints
{
    public class AuthEndpointsTests : IClassFixture<ConfereApiFactory>
    {
        private readonly HttpClient _cliente;

        public AuthEndpointsTests(ConfereApiFactory factory)
        {
            _cliente = factory.CreateClient();
        }

        [Fact]
        public async Task LoginCidadao_ComUsuarioDoSeed_Devolve200ComToken()
        {
            var resposta = await _cliente.EnviarAsync(HttpMethod.Post, "/auth/login", corpo: new { email = ClienteApi.CidadaoSeed, senha = ClienteApi.SenhaSeed });

            Assert.Equal(HttpStatusCode.OK, resposta.Status);
            Assert.Equal("Bearer", resposta.Corpo.GetProperty("tipo").GetString());
            Assert.Equal("CIDADAO", resposta.Corpo.GetProperty("usuario").GetProperty("perfil").GetString());
        }

        [Fact]
        public async Task LoginCidadao_SenhaErrada_Devolve401NoPadraoDeErro()
        {
            var resposta = await _cliente.EnviarAsync(HttpMethod.Post, "/auth/login", corpo: new { email = ClienteApi.CidadaoSeed, senha = "Errada@123" });

            Assert.Equal(HttpStatusCode.Unauthorized, resposta.Status);
            Assert.Equal("NAO_AUTENTICADO", resposta.Codigo);
            Assert.Equal("https://confere.app/erros/nao-autenticado", resposta.Corpo.GetProperty("type").GetString());
        }

        [Fact]
        public async Task Login_CorpoInvalido_Devolve400ComErrosPorCampo()
        {
            var resposta = await _cliente.EnviarAsync(HttpMethod.Post, "/auth/login", corpo: new { email = "nao-e-email", senha = "" });

            Assert.Equal(HttpStatusCode.BadRequest, resposta.Status);
            Assert.Equal("VALIDACAO", resposta.Codigo);
            Assert.Equal(2, resposta.Corpo.GetProperty("erros").GetArrayLength());
        }

        [Fact]
        public async Task LoginGestor_Devolve200ComPapelGestor()
        {
            var resposta = await _cliente.EnviarAsync(HttpMethod.Post, "/auth/token", corpo: new { email = ClienteApi.GestorSeed, senha = ClienteApi.SenhaSeed });

            Assert.Equal(HttpStatusCode.OK, resposta.Status);
            Assert.Equal("GESTOR", resposta.Corpo.GetProperty("usuario").GetProperty("perfil").GetString());
            Assert.Equal(3550308, resposta.Corpo.GetProperty("usuario").GetProperty("codIbgeMunicipio").GetInt32());
        }

        [Fact]
        public async Task Me_SemToken_Devolve401ComWwwAuthenticate()
        {
            var resposta = await _cliente.EnviarAsync(HttpMethod.Get, "/auth/me");

            Assert.Equal(HttpStatusCode.Unauthorized, resposta.Status);
            Assert.Equal("NAO_AUTENTICADO", resposta.Codigo);
            Assert.Equal("Bearer", resposta.Header("WWW-Authenticate"));
        }

        [Fact]
        public async Task Me_TokenDaApiJava_CriaOCidadaoAquiPeloEmail()
        {
            var email = ClienteApi.NovoEmail();

            var primeira = await _cliente.EnviarAsync(HttpMethod.Get, "/auth/me", TokenTeste.CidadaoDaApiJava(email));
            var segunda = await _cliente.EnviarAsync(HttpMethod.Get, "/auth/me", TokenTeste.CidadaoDaApiJava(email, idNoJava: 9999));

            Assert.Equal(HttpStatusCode.OK, primeira.Status);
            Assert.Equal(email, primeira.Corpo.GetProperty("email").GetString());
            Assert.Equal(primeira.Corpo.GetProperty("id").GetInt32(), segunda.Corpo.GetProperty("id").GetInt32());
        }

        [Fact]
        public async Task Me_TokenExpirado_Devolve401AvisandoQueExpirou()
        {
            var resposta = await _cliente.EnviarAsync(HttpMethod.Get, "/auth/me", TokenTeste.Expirado(ClienteApi.NovoEmail()));

            Assert.Equal(HttpStatusCode.Unauthorized, resposta.Status);
            Assert.Contains("expirou", resposta.Corpo.GetProperty("detail").GetString());
        }

        [Fact]
        public async Task Me_TokenDeEmissorDesconhecido_Devolve401()
        {
            var resposta = await _cliente.EnviarAsync(HttpMethod.Get, "/auth/me", TokenTeste.ComIssuerDesconhecido(ClienteApi.NovoEmail()));

            Assert.Equal(HttpStatusCode.Unauthorized, resposta.Status);
        }

        [Fact]
        public async Task Me_TokenAssinadoComOutraChave_Devolve401()
        {
            var resposta = await _cliente.EnviarAsync(HttpMethod.Get, "/auth/me", TokenTeste.AssinadoComOutraChave(ClienteApi.NovoEmail()));

            Assert.Equal(HttpStatusCode.Unauthorized, resposta.Status);
        }

        [Fact]
        public async Task Me_ComTokenDeGestor_Devolve403()
        {
            var gestor = await _cliente.LoginGestorSeedAsync();

            var resposta = await _cliente.EnviarAsync(HttpMethod.Get, "/auth/me", gestor);

            Assert.Equal(HttpStatusCode.Forbidden, resposta.Status);
            Assert.Equal("SEM_PERMISSAO", resposta.Codigo);
        }

        [Fact]
        public async Task Registrar_ContaNova_Devolve201EDepoisFazLogin()
        {
            var email = ClienteApi.NovoEmail();
            var cadastro = new { nome = "Maria Teste", email, senha = "Senha1234", telefone = "11912345678", codIbgeMunicipio = 3550308 };

            var registro = await _cliente.EnviarAsync(HttpMethod.Post, "/auth/registrar", corpo: cadastro);
            var login = await _cliente.EnviarAsync(HttpMethod.Post, "/auth/login", corpo: new { email, senha = "Senha1234" });
            var repetido = await _cliente.EnviarAsync(HttpMethod.Post, "/auth/registrar", corpo: cadastro);

            Assert.Equal(HttpStatusCode.Created, registro.Status);
            Assert.Equal(HttpStatusCode.OK, login.Status);
            Assert.Equal(HttpStatusCode.Conflict, repetido.Status);
        }

        [Fact]
        public async Task EndpointDeTokenDeTeste_NaoExisteForaDeDevelopment()
        {
            var resposta = await _cliente.EnviarAsync(HttpMethod.Post, "/auth/dev/token-cidadao-java",
                corpo: new { idUsuario = 1, nome = "X", email = "x@teste.com", codIbgeMunicipio = 3550308 });

            Assert.Equal(HttpStatusCode.NotFound, resposta.Status);
        }
    }
}
