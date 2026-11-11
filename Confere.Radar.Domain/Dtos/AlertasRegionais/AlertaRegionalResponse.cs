using Confere.Radar.Domain.Dtos.Radar;

namespace Confere.Radar.Domain.Dtos.AlertasRegionais
{
    public record AlertaRegionalResponse(
        int Id,
        string Titulo,
        string Mensagem,
        string Nivel,
        string Origem,
        TipoGolpeResumoResponse TipoGolpe,
        MunicipioResponse Municipio,
        DateTimeOffset DtInicio,
        DateTimeOffset DtFim,
        bool Ativo);
}
