using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Domain.Models;

namespace Confere.Radar.Application.Services
{
    public partial class NormalizadorIdentificador : INormalizadorIdentificador
    {
        private const string CampoValor = "valor";
        private const int TamanhoMaximoMascara = 120;

        public IdentificadorNormalizado Normalizar(string tipo, string valor)
        {
            var tipoNormalizado = (tipo ?? string.Empty).Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new ValidacaoException(CampoValor, "Informe o valor do identificador.");
            }

            var (normalizado, mascarado) = tipoNormalizado switch
            {
                TipoIdentificador.Telefone => NormalizarTelefone(valor),
                TipoIdentificador.Link => NormalizarLink(valor),
                TipoIdentificador.Email => NormalizarEmail(valor),
                TipoIdentificador.ChavePix => NormalizarChavePix(valor),
                TipoIdentificador.Perfil => NormalizarPerfil(valor),
                _ => throw new ValidacaoException("tipo", $"Tipo inválido. Use um destes: {string.Join(", ", TipoIdentificador.Todos)}.")
            };

            return new IdentificadorNormalizado(tipoNormalizado, normalizado, GerarHash(normalizado), Limitar(mascarado));
        }

        private static (string, string) NormalizarTelefone(string valor)
        {
            var digitos = new string(valor.Where(char.IsDigit).ToArray());

            if (digitos.Length is 10 or 11)
            {
                digitos = "55" + digitos;
            }

            if (digitos.Length is not (12 or 13) || !digitos.StartsWith("55"))
            {
                throw new ValidacaoException(CampoValor, "Telefone inválido. Informe DDD e número, por exemplo (11) 91234-5678.");
            }

            var ddd = digitos.Substring(2, 2);
            var numero = digitos[4..];
            var finalNumero = numero[^4..];

            var mascarado = numero.Length == 9
                ? $"+55 {ddd} {numero[0]}****-{finalNumero}"
                : $"+55 {ddd} ****-{finalNumero}";

            return (digitos, mascarado);
        }

        private static (string, string) NormalizarLink(string valor)
        {
            var link = valor.Trim().ToLowerInvariant();

            if (link.Any(char.IsWhiteSpace))
            {
                throw new ValidacaoException(CampoValor, "Link inválido. Ele não pode ter espaços.");
            }

            link = RemoverPrefixo(link, "https://");
            link = RemoverPrefixo(link, "http://");
            link = RemoverPrefixo(link, "www.");

            var indiceFragmento = link.IndexOf('#');
            if (indiceFragmento >= 0)
            {
                link = link[..indiceFragmento];
            }

            var partes = link.Split('?', 2);
            var caminho = partes[0].TrimEnd('/');

            var parametros = partes.Length > 1
                ? partes[1].Split('&', StringSplitOptions.RemoveEmptyEntries).Where(p => !p.StartsWith("utm_")).ToList()
                : new List<string>();

            var normalizado = parametros.Count > 0 ? $"{caminho}?{string.Join('&', parametros)}" : caminho;

            var dominio = caminho.Split('/', 2)[0];
            if (dominio.Length < 4 || !dominio.Contains('.') || dominio.StartsWith('.') || dominio.EndsWith('.'))
            {
                throw new ValidacaoException(CampoValor, "Link inválido. Informe um endereço como site.com.br/pagina.");
            }

            var mascarado = normalizado.Length > dominio.Length ? $"{dominio}/…" : dominio;

            return (normalizado, mascarado);
        }

        private static (string, string) NormalizarEmail(string valor)
        {
            var email = SemEspacos(valor).ToLowerInvariant();

            if (!EmailRegex().IsMatch(email))
            {
                throw new ValidacaoException(CampoValor, "E-mail inválido.");
            }

            var partes = email.Split('@');
            var usuario = partes[0];
            var inicio = usuario.Length > 2 ? usuario[..2] : usuario[..1];

            return (email, $"{inicio}***@{partes[1]}");
        }

        private static (string, string) NormalizarChavePix(string valor)
        {
            var chave = SemEspacos(valor).ToLowerInvariant();

            if (chave.Length < 5)
            {
                throw new ValidacaoException(CampoValor, "Chave Pix inválida.");
            }

            return (chave, MascararTexto(chave));
        }

        private static (string, string) NormalizarPerfil(string valor)
        {
            var perfil = SemEspacos(valor).ToLowerInvariant().TrimStart('@');

            if (perfil.Length < 2)
            {
                throw new ValidacaoException(CampoValor, "Perfil inválido. Informe o @ ou o link do perfil.");
            }

            return (perfil, "@" + MascararTexto(perfil));
        }

        private static string MascararTexto(string texto)
        {
            return texto.Length >= 8
                ? $"{texto[..3]}****{texto[^2..]}"
                : $"{texto[..1]}****";
        }

        private static string GerarHash(string valorNormalizado)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(valorNormalizado));
            return Convert.ToHexStringLower(bytes);
        }

        private static string RemoverPrefixo(string texto, string prefixo)
        {
            return texto.StartsWith(prefixo) ? texto[prefixo.Length..] : texto;
        }

        private static string SemEspacos(string texto)
        {
            return new string(texto.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        private static string Limitar(string texto)
        {
            return texto.Length <= TamanhoMaximoMascara ? texto : texto[..(TamanhoMaximoMascara - 1)] + "…";
        }

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial Regex EmailRegex();
    }
}
