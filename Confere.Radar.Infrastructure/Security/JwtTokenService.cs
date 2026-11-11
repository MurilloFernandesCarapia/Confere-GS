using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Confere.Radar.Application.Common;
using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Dtos.Auth;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Confere.Radar.Infrastructure.Security
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtSettings _settings;
        private readonly TimeProvider _timeProvider;

        public JwtTokenService(IOptions<JwtSettings> settings, TimeProvider timeProvider)
        {
            _settings = settings.Value;
            _timeProvider = timeProvider;
        }

        public TokenResponse GerarTokenGestor(Gestor gestor)
        {
            var municipio = gestor.Orgao?.Municipio;
            var claims = MontarClaims(gestor.IdGestor, gestor.Nome, gestor.Email, Papel.Gestor, municipio?.CodIbge);

            var (token, expiraEm) = CriarToken(_settings.IssuerRadar, claims);

            var usuario = new UsuarioLogadoResponse(
                gestor.IdGestor,
                gestor.Nome,
                gestor.Email,
                Papel.Gestor,
                municipio?.CodIbge,
                municipio?.Nome,
                gestor.Orgao?.Nome);

            return new TokenResponse(token, "Bearer", expiraEm, usuario);
        }

        public TokenResponse GerarTokenCidadao(Usuario usuario)
        {
            var municipio = usuario.Municipio;
            var claims = MontarClaims(usuario.IdUsuario, usuario.Nome, usuario.Email, Papel.Cidadao, municipio?.CodIbge);

            var (token, expiraEm) = CriarToken(_settings.IssuerRadar, claims);

            var usuarioLogado = new UsuarioLogadoResponse(
                usuario.IdUsuario,
                usuario.Nome,
                usuario.Email,
                Papel.Cidadao,
                municipio?.CodIbge,
                municipio?.Nome,
                null);

            return new TokenResponse(token, "Bearer", expiraEm, usuarioLogado);
        }

        public TokenCidadaoTesteResponse GerarTokenCidadaoTeste(TokenCidadaoTesteRequest request)
        {
            var claims = MontarClaims(request.IdUsuario, request.Nome, request.Email, Papel.Cidadao, request.CodIbgeMunicipio);

            var (token, expiraEm) = CriarToken(_settings.IssuerJava, claims);

            return new TokenCidadaoTesteResponse(
                token,
                "Bearer",
                expiraEm,
                _settings.IssuerJava,
                "Token de teste igual ao emitido pela API Java. Esse endpoint só existe em ambiente de desenvolvimento.");
        }

        private static List<Claim> MontarClaims(int id, string nome, string email, string papel, int? codIbgeMunicipio)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
                new Claim("nome", nome),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("role", papel)
            };

            if (codIbgeMunicipio.HasValue)
            {
                claims.Add(new Claim("municipio", codIbgeMunicipio.Value.ToString(), ClaimValueTypes.Integer32));
            }

            return claims;
        }

        private (string Token, DateTimeOffset ExpiraEm) CriarToken(string emissor, IEnumerable<Claim> claims)
        {
            var agora = _timeProvider.GetUtcNow();
            var expiraEm = agora.AddHours(_settings.ExpiracaoHoras);

            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
            var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: emissor,
                audience: _settings.Audience,
                claims: claims,
                notBefore: agora.UtcDateTime,
                expires: expiraEm.UtcDateTime,
                signingCredentials: credenciais);

            var tokenGerado = new JwtSecurityTokenHandler().WriteToken(token);

            return (tokenGerado, HorarioBrasilia.Converter(expiraEm.UtcDateTime));
        }
    }
}
