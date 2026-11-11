namespace Confere.Radar.Domain.Dtos.Beneficios
{
    public record BeneficioResumoResponse(
        string Codigo,
        string Nome,
        string OrgaoResponsavel,
        string DescricaoSimples,
        string? ValorReferencia,
        string LinkOficial);
}
