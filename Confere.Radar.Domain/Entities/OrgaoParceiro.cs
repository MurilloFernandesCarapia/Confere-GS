namespace Confere.Radar.Domain.Entities
{
    public class OrgaoParceiro
    {
        public int IdOrgao { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int IdMunicipio { get; set; }
        public string? EmailContato { get; set; }

        public Municipio? Municipio { get; set; }
        public ICollection<Gestor> Gestores { get; set; } = new List<Gestor>();
    }
}
