namespace Confere.Radar.Domain.Dtos.Direitos
{
    public record ElegibilidadeResponse(
        int Id,
        BeneficioElegibilidadeResponse Beneficio,
        string Status,
        string? Motivo,
        string Origem,
        DateTimeOffset AvaliadoEm,
        DateTimeOffset AtualizadoEm);
}
