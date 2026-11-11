using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Confere.Radar.IntegrationTests.Infra
{
    public static class TokenTeste
    {
        public const string Segredo = "segredo-usado-so-nos-testes-de-integracao-0123456789";
        public const string IssuerJava = "confere-java";
        public const string Audience = "confere-app";

        public static string CidadaoDaApiJava(string email, int codIbgeMunicipio = 3550308, int idNoJava = 5000)
        {
            return Gerar(IssuerJava, Segredo, idNoJava, "Cidadão de teste", email, "CIDADAO", codIbgeMunicipio, DateTime.UtcNow.AddHours(1));
        }

        public static string Expirado(string email)
        {
            return Gerar(IssuerJava, Segredo, 5000, "Expirado", email, "CIDADAO", 3550308, DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-3));
        }

        public static string ComIssuerDesconhecido(string email)
        {
            return Gerar("outra-api", Segredo, 5000, "Intruso", email, "CIDADAO", 3550308, DateTime.UtcNow.AddHours(1));
        }

        public static string AssinadoComOutraChave(string email)
        {
            return Gerar(IssuerJava, "uma-chave-totalmente-diferente-com-mais-de-32-bytes", 5000, "Intruso", email, "CIDADAO", 3550308, DateTime.UtcNow.AddHours(1));
        }

        private static string Gerar(string issuer, string segredo, int sub, string nome, string email, string papel, int codIbge, DateTime expira, DateTime? inicio = null)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, sub.ToString()),
                new("nome", nome),
                new(JwtRegisteredClaimNames.Email, email),
                new("role", papel),
                new("municipio", codIbge.ToString(), ClaimValueTypes.Integer32)
            };

            var credenciais = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(segredo)), SecurityAlgorithms.HmacSha256);
            var notBefore = inicio ?? DateTime.UtcNow.AddMinutes(-1);

            var token = new JwtSecurityToken(issuer, Audience, claims, notBefore, expira, credenciais);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
