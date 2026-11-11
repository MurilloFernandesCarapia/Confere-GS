namespace Confere.Radar.Domain.Dtos.Radar
{
    public record DenunciaResponse(
        int Id,
        IdentificadorResponse Identificador,
        TipoGolpeResumoResponse TipoGolpe,
        MunicipioResponse Municipio,
        string? Descricao,
        int? IdVerificacao,
        DateTimeOffset CriadoEm,
        DateTimeOffset? AtualizadoEm);
}
