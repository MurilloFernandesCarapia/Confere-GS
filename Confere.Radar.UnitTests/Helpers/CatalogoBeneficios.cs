using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Entities;

namespace Confere.Radar.UnitTests.Helpers
{
    public static class CatalogoBeneficios
    {
        public static Beneficio TarifaSocial() => Criar(1, "TARIFA_SOCIAL", "Tarifa Social de Energia",
            Requisito(1, CriterioRequisito.Cadunico, OperadorRequisito.Igual, "S"),
            Requisito(1, CriterioRequisito.RendaPcMax, OperadorRequisito.MenorOuIgual, FaixaRenda.AteMeio),
            Requisito(2, CriterioRequisito.Bpc, OperadorRequisito.Igual, "S"));

        public static Beneficio BpcIdoso() => Criar(2, "BPC_IDOSO", "BPC para pessoa idosa",
            Requisito(1, CriterioRequisito.IdadeMin, OperadorRequisito.MaiorOuIgual, "65"),
            Requisito(1, CriterioRequisito.RendaPcMax, OperadorRequisito.MenorOuIgual, FaixaRenda.AteUmQuarto),
            Requisito(1, CriterioRequisito.Cadunico, OperadorRequisito.Igual, "S"));

        public static Beneficio BpcPcd() => Criar(3, "BPC_PCD", "BPC para pessoa com deficiência",
            Requisito(1, CriterioRequisito.Deficiencia, OperadorRequisito.Igual, "S"),
            Requisito(1, CriterioRequisito.RendaPcMax, OperadorRequisito.MenorOuIgual, FaixaRenda.AteUmQuarto),
            Requisito(1, CriterioRequisito.Cadunico, OperadorRequisito.Igual, "S"));

        public static Beneficio ValoresAReceber() => Criar(4, "VALORES_A_RECEBER", "Valores a receber");

        public static Beneficio AbonoSalarial() => Criar(5, "ABONO_SALARIAL", "Abono salarial PIS/Pasep",
            Requisito(1, CriterioRequisito.VinculoFormal, OperadorRequisito.Igual, "S"));

        public static IReadOnlyList<Beneficio> Todos() => new[] { TarifaSocial(), BpcIdoso(), BpcPcd(), ValoresAReceber(), AbonoSalarial() };

        private static Beneficio Criar(int id, string codigo, string nome, params RequisitoBeneficio[] requisitos)
        {
            var beneficio = new Beneficio
            {
                IdBeneficio = id,
                Codigo = codigo,
                Nome = nome,
                OrgaoResponsavel = "Governo Federal",
                DescricaoSimples = nome,
                LinkOficial = "https://www.gov.br",
                Ativo = true
            };

            foreach (var requisito in requisitos)
            {
                requisito.IdBeneficio = id;
                beneficio.Requisitos.Add(requisito);
            }

            return beneficio;
        }

        private static RequisitoBeneficio Requisito(int grupo, string criterio, string operador, string valor)
        {
            return new RequisitoBeneficio { Grupo = grupo, Criterio = criterio, Operador = operador, Valor = valor };
        }
    }
}
