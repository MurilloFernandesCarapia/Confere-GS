using Confere.Radar.Domain.Constants;

namespace Confere.Radar.Domain.Entities
{
    public class IdentificadorSuspeito
    {
        public int IdIdentificador { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string ValorHash { get; set; } = string.Empty;
        public string ValorMascarado { get; set; } = string.Empty;
        public int TotalDenuncias { get; set; }
        public int Score { get; set; }
        public string Status { get; set; } = StatusIdentificador.EmAnalise;
        public DateTime DtPrimeira { get; set; }
        public DateTime DtUltima { get; set; }

        public ICollection<Denuncia> Denuncias { get; set; } = new List<Denuncia>();
    }
}
