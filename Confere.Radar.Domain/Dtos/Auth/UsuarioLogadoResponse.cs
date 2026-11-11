namespace Confere.Radar.Domain.Dtos.Auth
{
    public record UsuarioLogadoResponse(
        int Id,
        string Nome,
        string Email,
        string Perfil,
        int? CodIbgeMunicipio,
        string? Municipio,
        string? Orgao);
}
