namespace Confere.Radar.Domain.Entities
{
    public class RegistroIdempotencia
    {
        public string Chave { get; set; } = string.Empty;
        public int IdUsuarioExterno { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string HashRequisicao { get; set; } = string.Empty;
        public string Situacao { get; set; } = string.Empty;
        public int? StatusHttp { get; set; }
        public string? CorpoResposta { get; set; }
        public DateTime DtCriacao { get; set; }
        public DateTime DtExpiracao { get; set; }
    }
}
