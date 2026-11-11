namespace Confere.Radar.Domain.Dtos.Radar
{
    public record IdentificadorResponse(
        int Id,
        string Tipo,
        string ValorMascarado,
        int TotalDenuncias,
        int Score,
        string Status);
}
