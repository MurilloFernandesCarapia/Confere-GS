namespace Confere.Radar.Domain.Dtos.Radar
{
    public record ConsultaRadarResponse(
        bool Encontrado,
        string? Status,
        int TotalDenuncias,
        string? TipoMaisComum,
        DateTimeOffset? UltimaDenuncia);
}
