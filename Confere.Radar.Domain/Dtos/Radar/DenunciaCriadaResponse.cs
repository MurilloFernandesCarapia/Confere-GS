namespace Confere.Radar.Domain.Dtos.Radar
{
    public record DenunciaCriadaResponse(
        int Id,
        IdentificadorResponse Identificador,
        bool AlertaRegionalCriado,
        DateTimeOffset CriadoEm);
}
