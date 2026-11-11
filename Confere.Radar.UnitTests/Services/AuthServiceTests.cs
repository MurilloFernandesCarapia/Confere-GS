using Confere.Radar.Application.Services;
using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Dtos.Auth;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.UnitTests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Confere.Radar.UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
        private readonly Mock<IGestorRepository> _gestorRepository = new();
        private readonly Mock<IMunicipioRepository> _municipioRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<ISenhaHasher> _senhaHasher = new();
        private readonly Mock<ITokenService> _tokenService = new();
        private readonly AuthService _service;

        private readonly Municipio _saoPaulo = new() { IdMunicipio = 1, Nome = "São Paulo", Uf = "SP", CodIbge = 3550308 };

        public AuthServiceTests()
        {
            _municipioRepository.Setup(r => r.ObterPorCodIbgeAsync(3550308)).ReturnsAsync(_saoPaulo);
            _senhaHasher.Setup(h => h.GerarHash(It.IsAny<string>())).Returns("hash");
            _senhaHasher.Setup(h => h.Verificar("Senha@123", "hash")).Returns(true);

            _service = new AuthService(
                _usuarioRepository.Object,
                _gestorRepository.Object,
                _municipioRepository.Object,
                _unitOfWork.Object,
                _senhaHasher.Object,
                _tokenService.Object,
                new RelogioFixo(),
                NullLogger<AuthService>.Instance);
        }

        [Fact]
        public async Task Registrar_EmailNovo_CriaContaRadarComSenhaCriptografada()
        {
            Usuario? criado = null;
            _usuarioRepository.Setup(r => r.ObterPorEmailAsync("maria@teste.com")).ReturnsAsync((Usuario?)null);
            _usuarioRepository.Setup(r => r.AdicionarAsync(It.IsAny<Usuario>())).Callback<Usuario>(u => criado = u);

            var resposta = await _service.RegistrarCidadaoAsync(new RegistroCidadaoRequest(" Maria ", " Maria@Teste.com ", "Senha@123", null, 3550308));

            Assert.Equal("maria@teste.com", criado!.Email);
            Assert.Equal("hash", criado.SenhaHash);
            Assert.Equal(OrigemUsuario.Radar, criado.Origem);
            Assert.Equal("Maria", resposta.Nome);
            Assert.Equal(Papel.Cidadao, resposta.Perfil);
        }

        [Fact]
        public async Task Registrar_EmailComSenha_LancaConflito()
        {
            _usuarioRepository.Setup(r => r.ObterPorEmailAsync("maria@teste.com")).ReturnsAsync(new Usuario { Email = "maria@teste.com", SenhaHash = "hash" });

            await Assert.ThrowsAsync<ConflitoException>(() =>
                _service.RegistrarCidadaoAsync(new RegistroCidadaoRequest("Maria", "maria@teste.com", "Senha@123", null, 3550308)));
        }

        [Fact]
        public async Task Registrar_ContaQueVeioDaApiJava_SoDefineASenha()
        {
            var vindaDoJava = new Usuario { IdUsuario = 8, Email = "maria@teste.com", Origem = OrigemUsuario.Java };
            _usuarioRepository.Setup(r => r.ObterPorEmailAsync("maria@teste.com")).ReturnsAsync(vindaDoJava);

            await _service.RegistrarCidadaoAsync(new RegistroCidadaoRequest("Maria", "maria@teste.com", "Senha@123", null, 3550308));

            Assert.Equal("hash", vindaDoJava.SenhaHash);
            _usuarioRepository.Verify(r => r.AdicionarAsync(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task Registrar_MunicipioInexistente_LancaValidacao()
        {
            _municipioRepository.Setup(r => r.ObterPorCodIbgeAsync(1234567)).ReturnsAsync((Municipio?)null);

            await Assert.ThrowsAsync<ValidacaoException>(() =>
                _service.RegistrarCidadaoAsync(new RegistroCidadaoRequest("Maria", "maria@teste.com", "Senha@123", null, 1234567)));
        }

        [Theory]
        [InlineData("Errada@123")]
        [InlineData("")]
        public async Task LoginCidadao_SenhaErrada_LancaCredenciaisInvalidas(string senha)
        {
            _usuarioRepository.Setup(r => r.ObterPorEmailAsync("cida@confere.app")).ReturnsAsync(new Usuario { Email = "cida@confere.app", SenhaHash = "hash" });

            await Assert.ThrowsAsync<CredenciaisInvalidasException>(() => _service.LoginCidadaoAsync(new LoginRequest("cida@confere.app", senha)));
        }

        [Fact]
        public async Task LoginCidadao_ContaSemSenha_LancaCredenciaisInvalidas()
        {
            _usuarioRepository.Setup(r => r.ObterPorEmailAsync("java@teste.com")).ReturnsAsync(new Usuario { Email = "java@teste.com", SenhaHash = null });

            await Assert.ThrowsAsync<CredenciaisInvalidasException>(() => _service.LoginCidadaoAsync(new LoginRequest("java@teste.com", "Senha@123")));
        }

        [Fact]
        public async Task LoginGestor_Inativo_LancaCredenciaisInvalidas()
        {
            _gestorRepository.Setup(r => r.ObterPorEmailAsync("gestor@cras.sp.gov.br")).ReturnsAsync(new Gestor { SenhaHash = "hash", Ativo = false });

            await Assert.ThrowsAsync<CredenciaisInvalidasException>(() => _service.LoginGestorAsync(new LoginRequest("gestor@cras.sp.gov.br", "Senha@123")));
        }

        [Fact]
        public async Task LoginGestor_CredenciaisCertas_GeraToken()
        {
            var gestor = new Gestor { IdGestor = 1, SenhaHash = "hash", Ativo = true };
            _gestorRepository.Setup(r => r.ObterPorEmailAsync("gestor@cras.sp.gov.br")).ReturnsAsync(gestor);

            await _service.LoginGestorAsync(new LoginRequest("gestor@cras.sp.gov.br", "Senha@123"));

            _tokenService.Verify(t => t.GerarTokenGestor(gestor), Times.Once);
        }

        [Fact]
        public async Task ResolverCidadaoDaApiJava_EmailJaExiste_DevolveOMesmoId()
        {
            _usuarioRepository.Setup(r => r.ObterPorEmailAsync("cida@confere.app")).ReturnsAsync(new Usuario { IdUsuario = 1 });

            var id = await _service.ResolverCidadaoDaApiJavaAsync(" CIDA@confere.app ", "Cida", 3550308);

            Assert.Equal(1, id);
            _usuarioRepository.Verify(r => r.AdicionarAsync(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task ResolverCidadaoDaApiJava_EmailNovo_CriaUsuarioComOrigemJava()
        {
            Usuario? criado = null;
            _usuarioRepository.Setup(r => r.ObterPorEmailAsync("novo@teste.com")).ReturnsAsync((Usuario?)null);
            _usuarioRepository.Setup(r => r.AdicionarAsync(It.IsAny<Usuario>())).Callback<Usuario>(u => criado = u);

            await _service.ResolverCidadaoDaApiJavaAsync("novo@teste.com", "Novo", 3550308);

            Assert.Equal(OrigemUsuario.Java, criado!.Origem);
            Assert.Null(criado.SenhaHash);
            Assert.Equal(1, criado.IdMunicipio);
        }

        [Fact]
        public async Task ObterCidadao_Inexistente_LancaNaoEncontrado()
        {
            _usuarioRepository.Setup(r => r.ObterPorIdAsync(99)).ReturnsAsync((Usuario?)null);

            await Assert.ThrowsAsync<NaoEncontradoException>(() => _service.ObterCidadaoAsync(99));
        }
    }
}
