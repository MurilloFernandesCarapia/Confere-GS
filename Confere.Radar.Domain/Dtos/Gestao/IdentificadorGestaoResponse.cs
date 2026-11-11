namespace Confere.Radar.Domain.Dtos.Gestao
{
    public record IdentificadorGestaoResponse(
        int Id,
        string Tipo,
        string ValorMascarado,
        int TotalDenuncias,
        int Score,
        string Status,
        DateTimeOffset PrimeiraDenuncia,
        DateTimeOffset UltimaDenuncia);
}
