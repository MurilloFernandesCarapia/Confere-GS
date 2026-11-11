using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Entities;

namespace Confere.Radar.Application.Common
{
    public static class DescricaoRequisito
    {
        public static string Descrever(RequisitoBeneficio requisito)
        {
            return requisito.Criterio switch
            {
                CriterioRequisito.IdadeMin => $"Ter {requisito.Valor} anos ou mais",
                CriterioRequisito.RendaPcMax => $"Renda por pessoa de {DescreverFaixa(requisito.Valor)}",
                CriterioRequisito.Cadunico => "Estar inscrito no CadÚnico",
                CriterioRequisito.Bpc => "Receber o BPC",
                CriterioRequisito.Deficiencia => "Ter alguma deficiência",
                CriterioRequisito.VinculoFormal => "Ter trabalhado com carteira assinada",
                _ => $"{requisito.Criterio} {requisito.Operador} {requisito.Valor}"
            };
        }

        public static string DescreverFaixa(string faixa)
        {
            return faixa switch
            {
                FaixaRenda.AteUmQuarto => "até 1/4 do salário mínimo",
                FaixaRenda.AteMeio => "até meio salário mínimo",
                FaixaRenda.AteUm => "até 1 salário mínimo",
                FaixaRenda.AteDois => "até 2 salários mínimos",
                FaixaRenda.AcimaDeDois => "mais de 2 salários mínimos",
                _ => faixa
            };
        }
    }
}
