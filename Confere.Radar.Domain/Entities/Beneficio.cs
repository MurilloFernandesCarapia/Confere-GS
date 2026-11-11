namespace Confere.Radar.Domain.Entities
{
    public class Beneficio
    {
        public int IdBeneficio { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string OrgaoResponsavel { get; set; } = string.Empty;
        public string DescricaoSimples { get; set; } = string.Empty;
        public string LinkOficial { get; set; } = string.Empty;
        public string? ValorReferencia { get; set; }
        public bool Ativo { get; set; } = true;

        public ICollection<RequisitoBeneficio> Requisitos { get; set; } = new List<RequisitoBeneficio>();
        public ICollection<PassoBeneficio> Passos { get; set; } = new List<PassoBeneficio>();
    }
}
