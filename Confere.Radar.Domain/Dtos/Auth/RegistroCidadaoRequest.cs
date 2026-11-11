namespace Confere.Radar.Domain.Dtos.Auth
{
    public record RegistroCidadaoRequest(string Nome, string Email, string Senha, string? Telefone, int CodIbgeMunicipio);
}
