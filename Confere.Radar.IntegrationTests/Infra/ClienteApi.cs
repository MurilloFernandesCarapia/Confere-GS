using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Confere.Radar.IntegrationTests.Infra
{
    public record RespostaApi(HttpStatusCode Status, JsonElement Corpo, HttpResponseMessage Mensagem)
    {
        public string? Header(string nome)
        {
            if (Mensagem.Headers.TryGetValues(nome, out var valores))
            {
                return string.Join(",", valores);
            }

            return Mensagem.Content.Headers.TryGetValues(nome, out var valoresConteudo) ? string.Join(",", valoresConteudo) : null;
        }

        public string? Codigo => Corpo.ValueKind == JsonValueKind.Object && Corpo.TryGetProperty("codigo", out var codigo) ? codigo.GetString() : null;
    }

    public static class ClienteApi
    {
        public const string CidadaoSeed = "cida@confere.app";
        public const string GestorSeed = "gestor@cras.sp.gov.br";
        public const string SenhaSeed = "Senha@123";

        public static async Task<RespostaApi> EnviarAsync(this HttpClient cliente, HttpMethod metodo, string url, string? token = null, object? corpo = null, string? idempotencyKey = null)
        {
            using var requisicao = new HttpRequestMessage(metodo, url);

            if (token is not null)
            {
                requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if (idempotencyKey is not null)
            {
                requisicao.Headers.Add("Idempotency-Key", idempotencyKey);
            }

            if (corpo is not null)
            {
                requisicao.Content = JsonContent.Create(corpo);
            }

            var resposta = await cliente.SendAsync(requisicao);
            var texto = await resposta.Content.ReadAsStringAsync();
            var json = string.IsNullOrWhiteSpace(texto) ? default : JsonDocument.Parse(texto).RootElement.Clone();

            return new RespostaApi(resposta.StatusCode, json, resposta);
        }

        public static async Task<string> LoginCidadaoSeedAsync(this HttpClient cliente)
        {
            var resposta = await cliente.EnviarAsync(HttpMethod.Post, "/auth/login", corpo: new { email = CidadaoSeed, senha = SenhaSeed });
            return resposta.Corpo.GetProperty("token").GetString()!;
        }

        public static async Task<string> LoginGestorSeedAsync(this HttpClient cliente)
        {
            var resposta = await cliente.EnviarAsync(HttpMethod.Post, "/auth/token", corpo: new { email = GestorSeed, senha = SenhaSeed });
            return resposta.Corpo.GetProperty("token").GetString()!;
        }

        public static string NovoEmail() => $"teste-{Guid.NewGuid():N}@teste.com";

        public static string NovaChave() => Guid.NewGuid().ToString();

        public static string NovoTelefone() => $"119{Random.Shared.Next(10000000, 99999999)}";

        public static object NovaDenuncia(string telefone, string tipoGolpe = "FALSA_CENTRAL", int codIbge = 3550308)
        {
            return new { tipo = "TELEFONE", valor = telefone, tipoGolpeCodigo = tipoGolpe, codIbgeMunicipio = codIbge, descricao = "Ligaram dizendo ser do banco" };
        }
    }
}
