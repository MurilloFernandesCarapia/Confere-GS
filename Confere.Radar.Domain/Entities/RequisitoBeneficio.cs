namespace Confere.Radar.Domain.Entities
{
    public class RequisitoBeneficio
    {
        public int IdRequisito { get; set; }
        public int IdBeneficio { get; set; }
        public int Grupo { get; set; }
        public string Criterio { get; set; } = string.Empty;
        public string Operador { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;

        public Beneficio? Beneficio { get; set; }
    }
}
