namespace Confere.Radar.Domain.Dtos.Auth
{
    public record TokenResponse(string Token, string Tipo, DateTimeOffset ExpiraEm, UsuarioLogadoResponse Usuario);
}
