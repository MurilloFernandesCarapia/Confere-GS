namespace Confere.Radar.Domain.Entities
{
    public class PerfilSocial
    {
        public int IdUsuario { get; set; }
        public int? Idade { get; set; }
        public string? RendaPcFaixa { get; set; }
        public int? QtdMoradores { get; set; }
        public bool? InscritoCadunico { get; set; }
        public bool? RecebeBpc { get; set; }
        public bool? Deficiencia { get; set; }
        public bool? VinculoFormal { get; set; }
        public int? IdMunicipio { get; set; }
        public DateTime DtConsentimento { get; set; }
        public DateTime DtAtualizacao { get; set; }

        public Usuario? Usuario { get; set; }
        public Municipio? Municipio { get; set; }
    }
}
