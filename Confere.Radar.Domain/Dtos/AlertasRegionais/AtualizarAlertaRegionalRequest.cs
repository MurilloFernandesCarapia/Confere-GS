namespace Confere.Radar.Domain.Dtos.AlertasRegionais
{
    public record AtualizarAlertaRegionalRequest(
        string TipoGolpeCodigo,
        string Titulo,
        string Mensagem,
        string Nivel,
        DateTimeOffset DtFim);
}
