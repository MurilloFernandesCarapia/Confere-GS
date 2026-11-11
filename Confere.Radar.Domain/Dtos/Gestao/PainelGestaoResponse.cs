using Confere.Radar.Domain.Dtos.Radar;
using Confere.Radar.Domain.Models;

namespace Confere.Radar.Domain.Dtos.Gestao
{
    public record PainelGestaoResponse(
        MunicipioResponse Municipio,
        DateTimeOffset De,
        DateTimeOffset Ate,
        int TotalDenuncias,
        int IdentificadoresConfirmados,
        int IdentificadoresEmAnalise,
        int IdentificadoresDescartados,
        int AlertasEmitidos,
        int AlertasAtivos,
        IReadOnlyList<TotalPorTipoGolpe> TiposMaisDenunciados);
}
