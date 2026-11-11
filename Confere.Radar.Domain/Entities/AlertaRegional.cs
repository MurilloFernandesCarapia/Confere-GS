namespace Confere.Radar.Domain.Entities
{
    public class AlertaRegional
    {
        public int IdAlertaRegional { get; set; }
        public int IdMunicipio { get; set; }
        public int IdTipoGolpe { get; set; }
        public int? IdGestor { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
        public DateTime DtInicio { get; set; }
        public DateTime DtFim { get; set; }
        public bool Ativo { get; set; } = true;

        public Municipio? Municipio { get; set; }
        public TipoGolpe? TipoGolpe { get; set; }
        public Gestor? Gestor { get; set; }
    }
}
