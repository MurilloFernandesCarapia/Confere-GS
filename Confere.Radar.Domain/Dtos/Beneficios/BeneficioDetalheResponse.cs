namespace Confere.Radar.Domain.Dtos.Beneficios
{
    public record BeneficioDetalheResponse(
        string Codigo,
        string Nome,
        string OrgaoResponsavel,
        string DescricaoSimples,
        string? ValorReferencia,
        string LinkOficial,
        IReadOnlyList<RequisitoBeneficioResponse> Requisitos,
        IReadOnlyList<PassoBeneficioResponse> Passos);
}
