namespace Confere.Radar.Domain.Entities
{
    public class Elegibilidade
    {
        public int IdElegibilidade { get; set; }
        public int IdUsuarioExterno { get; set; }
        public int IdBeneficio { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Motivo { get; set; }
        public string Origem { get; set; } = string.Empty;
        public DateTime DtAvaliacao { get; set; }
        public DateTime DtAtualizacao { get; set; }

        public Beneficio? Beneficio { get; set; }
    }
}
