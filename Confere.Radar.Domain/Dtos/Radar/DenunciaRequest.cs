namespace Confere.Radar.Domain.Dtos.Radar
{
    public record DenunciaRequest(
        string Tipo,
        string Valor,
        string TipoGolpeCodigo,
        int CodIbgeMunicipio,
        string? Descricao,
        int? IdVerificacao);
}
