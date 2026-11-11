namespace Confere.Radar.Domain.Entities
{
    public class Municipio
    {
        public int IdMunicipio { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Uf { get; set; } = string.Empty;
        public int CodIbge { get; set; }
    }
}
