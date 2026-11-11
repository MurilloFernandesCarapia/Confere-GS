namespace Confere.Radar.Domain.Entities
{
    public class Denuncia
    {
        public int IdDenuncia { get; set; }
        public int IdUsuario { get; set; }
        public int IdIdentificador { get; set; }
        public int IdTipoGolpe { get; set; }
        public int IdMunicipio { get; set; }
        public int? IdVerificacaoExterna { get; set; }
        public string? Descricao { get; set; }
        public DateTime DtDenuncia { get; set; }
        public DateTime? DtAtualizacao { get; set; }

        public Usuario? Usuario { get; set; }
        public IdentificadorSuspeito? Identificador { get; set; }
        public TipoGolpe? TipoGolpe { get; set; }
        public Municipio? Municipio { get; set; }
    }
}
