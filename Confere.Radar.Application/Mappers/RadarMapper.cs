using Confere.Radar.Application.Common;
using Confere.Radar.Domain.Dtos.AlertasRegionais;
using Confere.Radar.Domain.Dtos.Gestao;
using Confere.Radar.Domain.Dtos.Radar;
using Confere.Radar.Domain.Entities;

namespace Confere.Radar.Application.Mappers
{
    public static class RadarMapper
    {
        public static IdentificadorResponse ParaResposta(IdentificadorSuspeito identificador)
        {
            return new IdentificadorResponse(
                identificador.IdIdentificador,
                identificador.Tipo,
                identificador.ValorMascarado,
                identificador.TotalDenuncias,
                identificador.Score,
                identificador.Status);
        }

        public static IdentificadorGestaoResponse ParaRespostaGestao(IdentificadorSuspeito identificador)
        {
            return new IdentificadorGestaoResponse(
                identificador.IdIdentificador,
                identificador.Tipo,
                identificador.ValorMascarado,
                identificador.TotalDenuncias,
                identificador.Score,
                identificador.Status,
                HorarioBrasilia.Converter(identificador.DtPrimeira),
                HorarioBrasilia.Converter(identificador.DtUltima));
        }

        public static TipoGolpeResponse ParaResposta(TipoGolpe tipoGolpe)
        {
            return new TipoGolpeResponse(
                tipoGolpe.Codigo,
                tipoGolpe.Nome,
                tipoGolpe.Gravidade,
                tipoGolpe.Descricao,
                tipoGolpe.OrgaoImitado);
        }

        public static TipoGolpeResumoResponse ParaResumo(TipoGolpe tipoGolpe)
        {
            return new TipoGolpeResumoResponse(tipoGolpe.Codigo, tipoGolpe.Nome);
        }

        public static MunicipioResponse ParaResposta(Municipio municipio)
        {
            return new MunicipioResponse(municipio.CodIbge, municipio.Nome, municipio.Uf);
        }

        public static DenunciaResponse ParaResposta(Denuncia denuncia)
        {
            return new DenunciaResponse(
                denuncia.IdDenuncia,
                ParaResposta(denuncia.Identificador!),
                ParaResumo(denuncia.TipoGolpe!),
                ParaResposta(denuncia.Municipio!),
                denuncia.Descricao,
                denuncia.IdVerificacaoExterna,
                HorarioBrasilia.Converter(denuncia.DtDenuncia),
                HorarioBrasilia.Converter(denuncia.DtAtualizacao));
        }

        public static AlertaRegionalResponse ParaResposta(AlertaRegional alerta)
        {
            return new AlertaRegionalResponse(
                alerta.IdAlertaRegional,
                alerta.Titulo,
                alerta.Mensagem,
                alerta.Nivel,
                alerta.Origem,
                ParaResumo(alerta.TipoGolpe!),
                ParaResposta(alerta.Municipio!),
                HorarioBrasilia.Converter(alerta.DtInicio),
                HorarioBrasilia.Converter(alerta.DtFim),
                alerta.Ativo);
        }
    }
}
