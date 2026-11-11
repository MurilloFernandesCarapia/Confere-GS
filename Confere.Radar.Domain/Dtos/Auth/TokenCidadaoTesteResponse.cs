namespace Confere.Radar.Domain.Dtos.Auth
{
    public record TokenCidadaoTesteResponse(string Token, string Tipo, DateTimeOffset ExpiraEm, string Emissor, string Observacao);
}
