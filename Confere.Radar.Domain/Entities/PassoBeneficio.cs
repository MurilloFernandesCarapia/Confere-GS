namespace Confere.Radar.Domain.Entities
{
    public class PassoBeneficio
    {
        public int IdPasso { get; set; }
        public int IdBeneficio { get; set; }
        public int Ordem { get; set; }
        public string Instrucao { get; set; } = string.Empty;
        public string Canal { get; set; } = string.Empty;

        public Beneficio? Beneficio { get; set; }
    }
}
