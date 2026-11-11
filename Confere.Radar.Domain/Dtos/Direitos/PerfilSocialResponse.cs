using Confere.Radar.Domain.Dtos.Radar;

namespace Confere.Radar.Domain.Dtos.Direitos
{
    public record PerfilSocialResponse(
        int? Idade,
        string? RendaPcFaixa,
        string? RendaPcFaixaDescricao,
        int? QtdMoradores,
        bool? InscritoCadunico,
        bool? RecebeBpc,
        bool? Deficiencia,
        bool? VinculoFormal,
        MunicipioResponse? Municipio,
        DateTimeOffset ConsentidoEm,
        DateTimeOffset AtualizadoEm);
}
