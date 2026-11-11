namespace Confere.Radar.Domain.Entities
{
    public class TipoGolpe
    {
        public int IdTipoGolpe { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public int Gravidade { get; set; }
        public string? OrgaoImitado { get; set; }
    }
}
