namespace Confere.Radar.Domain.Dtos.AlertasRegionais
{
    public record AlertaRegionalRequest(
        int CodIbgeMunicipio,
        string TipoGolpeCodigo,
        string Titulo,
        string Mensagem,
        string Nivel,
        DateTimeOffset? DtInicio,
        DateTimeOffset DtFim);
}
