namespace Confere.Radar.Domain.Dtos.Radar
{
    public record TipoGolpeResponse(
        string Codigo,
        string Nome,
        int Gravidade,
        string? Descricao,
        string? OrgaoImitado);
}
