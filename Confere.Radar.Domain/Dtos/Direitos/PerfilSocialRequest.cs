namespace Confere.Radar.Domain.Dtos.Direitos
{
    public record PerfilSocialRequest(
        int? Idade,
        string? RendaPcFaixa,
        int? QtdMoradores,
        bool? InscritoCadunico,
        bool? RecebeBpc,
        bool? Deficiencia,
        bool? VinculoFormal,
        int? CodIbgeMunicipio,
        bool Consentimento);
}
