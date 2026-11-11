using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Dtos.Auth;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Confere.Radar.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IGestorRepository _gestorRepository;
        private readonly IMunicipioRepository _municipioRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISenhaHasher _senhaHasher;
        private readonly ITokenService _tokenService;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUsuarioRepository usuarioRepository,
            IGestorRepository gestorRepository,
            IMunicipioRepository municipioRepository,
            IUnitOfWork unitOfWork,
            ISenhaHasher senhaHasher,
            ITokenService tokenService,
            TimeProvider timeProvider,
            ILogger<AuthService> logger)
        {
            _usuarioRepository = usuarioRepository;
            _gestorRepository = gestorRepository;
            _municipioRepository = municipioRepository;
            _unitOfWork = unitOfWork;
            _senhaHasher = senhaHasher;
            _tokenService = tokenService;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<UsuarioLogadoResponse> RegistrarCidadaoAsync(RegistroCidadaoRequest request)
        {
            var municipio = await _municipioRepository.ObterPorCodIbgeAsync(request.CodIbgeMunicipio);
            if (municipio is null)
            {
                throw new ValidacaoException("codIbgeMunicipio", "Município não encontrado.");
            }

            var email = request.Email.Trim().ToLowerInvariant();
            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);

            if (usuario is not null && usuario.SenhaHash is not null)
            {
                throw new ConflitoException("Já existe uma conta com esse e-mail.");
            }

            if (usuario is null)
            {
                usuario = new Usuario
                {
                    Email = email,
                    Origem = OrigemUsuario.Radar,
                    DtCadastro = _timeProvider.GetUtcNow().UtcDateTime
                };

                await _usuarioRepository.AdicionarAsync(usuario);
            }

            usuario.Nome = request.Nome.Trim();
            usuario.Telefone = request.Telefone?.Trim();
            usuario.IdMunicipio = municipio.IdMunicipio;
            usuario.Municipio = municipio;
            usuario.SenhaHash = _senhaHasher.GerarHash(request.Senha);

            await _unitOfWork.SalvarAsync();

            _logger.LogInformation("Cidadão {IdUsuario} cadastrado", usuario.IdUsuario);
            return ParaResposta(usuario);
        }

        public async Task<TokenResponse> LoginCidadaoAsync(LoginRequest request)
        {
            var usuario = await _usuarioRepository.ObterPorEmailAsync(request.Email);

            if (usuario is null || !usuario.Ativo || usuario.SenhaHash is null || !_senhaHasher.Verificar(request.Senha, usuario.SenhaHash))
            {
                _logger.LogWarning("Tentativa de login de cidadão recusada para {Email}", request.Email);
                throw new CredenciaisInvalidasException();
            }

            _logger.LogInformation("Cidadão {IdUsuario} fez login", usuario.IdUsuario);
            return _tokenService.GerarTokenCidadao(usuario);
        }

        public async Task<TokenResponse> LoginGestorAsync(LoginRequest request)
        {
            var gestor = await _gestorRepository.ObterPorEmailAsync(request.Email);

            if (gestor is null || !gestor.Ativo || !_senhaHasher.Verificar(request.Senha, gestor.SenhaHash))
            {
                _logger.LogWarning("Tentativa de login de gestor recusada para {Email}", request.Email);
                throw new CredenciaisInvalidasException();
            }

            _logger.LogInformation("Gestor {IdGestor} fez login", gestor.IdGestor);
            return _tokenService.GerarTokenGestor(gestor);
        }

        public async Task<UsuarioLogadoResponse> ObterCidadaoAsync(int idUsuario)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(idUsuario);
            if (usuario is null)
            {
                throw new NaoEncontradoException("Usuário não encontrado.");
            }

            return ParaResposta(usuario);
        }

        public async Task<int> ResolverCidadaoDaApiJavaAsync(string email, string nome, int? codIbgeMunicipio)
        {
            var emailNormalizado = email.Trim().ToLowerInvariant();
            var usuario = await _usuarioRepository.ObterPorEmailAsync(emailNormalizado);

            if (usuario is not null)
            {
                return usuario.IdUsuario;
            }

            var municipio = codIbgeMunicipio.HasValue
                ? await _municipioRepository.ObterPorCodIbgeAsync(codIbgeMunicipio.Value)
                : null;

            usuario = new Usuario
            {
                Nome = nome,
                Email = emailNormalizado,
                IdMunicipio = municipio?.IdMunicipio,
                Origem = OrigemUsuario.Java,
                DtCadastro = _timeProvider.GetUtcNow().UtcDateTime
            };

            await _usuarioRepository.AdicionarAsync(usuario);
            await _unitOfWork.SalvarAsync();

            _logger.LogInformation("Cidadão {IdUsuario} criado a partir de um token da API Java", usuario.IdUsuario);
            return usuario.IdUsuario;
        }

        private static UsuarioLogadoResponse ParaResposta(Usuario usuario)
        {
            return new UsuarioLogadoResponse(
                usuario.IdUsuario,
                usuario.Nome,
                usuario.Email,
                Papel.Cidadao,
                usuario.Municipio?.CodIbge,
                usuario.Municipio?.Nome,
                null);
        }
    }
}
