namespace Confere.Radar.Domain.Models
{
    public record EstatisticaIdentificador(int TotalDenuncias, int MunicipiosDistintos, DateTime? PrimeiraDenuncia, DateTime? UltimaDenuncia);
}
