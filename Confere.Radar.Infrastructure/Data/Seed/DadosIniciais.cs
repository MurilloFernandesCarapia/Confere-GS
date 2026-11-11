using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Entities;

namespace Confere.Radar.Infrastructure.Data.Seed
{
    public static class DadosIniciais
    {
        private static readonly DateTime DataCarga = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);

        public static readonly Municipio[] Municipios =
        {
            new Municipio { IdMunicipio = 1, Nome = "São Paulo", Uf = "SP", CodIbge = 3550308 },
            new Municipio { IdMunicipio = 2, Nome = "Guarulhos", Uf = "SP", CodIbge = 3518800 },
            new Municipio { IdMunicipio = 3, Nome = "Campinas", Uf = "SP", CodIbge = 3509502 },
            new Municipio { IdMunicipio = 4, Nome = "Osasco", Uf = "SP", CodIbge = 3534401 },
            new Municipio { IdMunicipio = 5, Nome = "Santo André", Uf = "SP", CodIbge = 3547809 },
            new Municipio { IdMunicipio = 6, Nome = "Rio de Janeiro", Uf = "RJ", CodIbge = 3304557 },
            new Municipio { IdMunicipio = 7, Nome = "Belo Horizonte", Uf = "MG", CodIbge = 3106200 },
            new Municipio { IdMunicipio = 8, Nome = "Curitiba", Uf = "PR", CodIbge = 4106902 },
            new Municipio { IdMunicipio = 9, Nome = "Porto Alegre", Uf = "RS", CodIbge = 4314902 },
            new Municipio { IdMunicipio = 10, Nome = "Salvador", Uf = "BA", CodIbge = 2927408 },
            new Municipio { IdMunicipio = 11, Nome = "Recife", Uf = "PE", CodIbge = 2611606 },
            new Municipio { IdMunicipio = 12, Nome = "Fortaleza", Uf = "CE", CodIbge = 2304400 },
            new Municipio { IdMunicipio = 13, Nome = "Brasília", Uf = "DF", CodIbge = 5300108 }
        };

        public static readonly TipoGolpe[] TiposGolpe =
        {
            new TipoGolpe { IdTipoGolpe = 1, Codigo = "FALSA_PROVA_VIDA", Nome = "Falsa prova de vida", Gravidade = 5, OrgaoImitado = "INSS",
                Descricao = "Pede selfie ou foto de documento dizendo que o benefício vai ser bloqueado. O INSS não faz prova de vida por WhatsApp." },
            new TipoGolpe { IdTipoGolpe = 2, Codigo = "FALSO_REEMBOLSO_INSS", Nome = "Falsa devolução de descontos do INSS", Gravidade = 5, OrgaoImitado = "INSS",
                Descricao = "Promete devolver descontos indevidos do INSS e pede dados ou pagamento de taxa." },
            new TipoGolpe { IdTipoGolpe = 3, Codigo = "FALSA_CENTRAL", Nome = "Falsa central do banco", Gravidade = 5, OrgaoImitado = "Bancos",
                Descricao = "Liga dizendo ser do banco, fala de compra suspeita e pede senha, código ou transferência." },
            new TipoGolpe { IdTipoGolpe = 4, Codigo = "FALSO_PARENTE", Nome = "\"Oi mãe, troquei de número\"", Gravidade = 4, OrgaoImitado = "Família",
                Descricao = "Alguém finge ser um parente com número novo e pede dinheiro com urgência." },
            new TipoGolpe { IdTipoGolpe = 5, Codigo = "FALSO_VALORES_RECEBER", Nome = "Falso resgate de valores esquecidos", Gravidade = 4, OrgaoImitado = "Banco Central",
                Descricao = "Diz que você tem dinheiro esquecido e cobra taxa para liberar. O resgate oficial é gratuito." },
            new TipoGolpe { IdTipoGolpe = 6, Codigo = "FALSO_EMPRESTIMO", Nome = "Empréstimo consignado falso ou não autorizado", Gravidade = 4, OrgaoImitado = "Bancos / INSS",
                Descricao = "Oferece empréstimo com condições boas demais ou contrata sem autorização." },
            new TipoGolpe { IdTipoGolpe = 7, Codigo = "DEEPFAKE_CELEBRIDADE", Nome = "Vídeo falso de jornalista ou famoso", Gravidade = 4, OrgaoImitado = "Imprensa",
                Descricao = "Vídeo feito com IA de alguém conhecido prometendo dinheiro ou benefício." },
            new TipoGolpe { IdTipoGolpe = 8, Codigo = "FALSO_BOLETO", Nome = "Boleto ou conta adulterada", Gravidade = 3, OrgaoImitado = "Concessionárias",
                Descricao = "Boleto com dados trocados para o dinheiro ir para outra pessoa." },
            new TipoGolpe { IdTipoGolpe = 9, Codigo = "LINK_RASTREIO", Nome = "Falsa taxa de entrega ou rastreio", Gravidade = 3, OrgaoImitado = "Correios",
                Descricao = "Mensagem com link cobrando taxa para liberar uma encomenda." },
            new TipoGolpe { IdTipoGolpe = 10, Codigo = "PIX_ERRADO", Nome = "\"Te mandei um Pix por engano\"", Gravidade = 3,
                Descricao = "Pede para devolver um Pix que nunca foi feito ou que vai ser estornado depois." }
        };

        public static readonly OrgaoParceiro[] OrgaosParceiros =
        {
            new OrgaoParceiro { IdOrgao = 1, Nome = "CRAS Centro - São Paulo", Tipo = TipoOrgao.Cras, IdMunicipio = 1 }
        };

        public static readonly Gestor[] Gestores =
        {
            new Gestor
            {
                IdGestor = 1,
                IdOrgao = 1,
                Nome = "Gestor de Teste CRAS",
                Cargo = "Coordenador",
                Email = "gestor@cras.sp.gov.br",
                SenhaHash = "$2a$11$lckz4ODK.px//fnKXwwHuuE0ddFgWQH09d6bTCdx23To3ppoHePAy",
                Ativo = true,
                DtCadastro = DataCarga
            }
        };

        public static readonly Usuario[] Usuarios =
        {
            new Usuario
            {
                IdUsuario = 1,
                Nome = "Maria Aparecida",
                Email = "cida@confere.app",
                SenhaHash = "$2a$11$lckz4ODK.px//fnKXwwHuuE0ddFgWQH09d6bTCdx23To3ppoHePAy",
                Telefone = "11987654321",
                IdMunicipio = 1,
                Origem = OrigemUsuario.Radar,
                Ativo = true,
                DtCadastro = DataCarga
            }
        };

        public static readonly Beneficio[] Beneficios =
        {
            new Beneficio
            {
                IdBeneficio = 1,
                Codigo = "TARIFA_SOCIAL",
                Nome = "Tarifa Social de Energia",
                OrgaoResponsavel = "ANEEL e distribuidora de energia",
                DescricaoSimples = "Desconto na conta de luz para famílias de baixa renda inscritas no CadÚnico ou que recebem BPC.",
                LinkOficial = "https://www.gov.br/aneel",
                ValorReferencia = "Até 65% de desconto na conta"
            },
            new Beneficio
            {
                IdBeneficio = 2,
                Codigo = "BPC_IDOSO",
                Nome = "BPC para pessoa idosa",
                OrgaoResponsavel = "INSS",
                DescricaoSimples = "Um salário mínimo por mês para quem tem 65 anos ou mais e renda familiar baixa. Não precisa ter contribuído com o INSS.",
                LinkOficial = "https://www.gov.br/inss",
                ValorReferencia = "1 salário mínimo por mês"
            },
            new Beneficio
            {
                IdBeneficio = 3,
                Codigo = "BPC_PCD",
                Nome = "BPC para pessoa com deficiência",
                OrgaoResponsavel = "INSS",
                DescricaoSimples = "Um salário mínimo por mês para pessoas com deficiência de qualquer idade e renda familiar baixa.",
                LinkOficial = "https://www.gov.br/inss",
                ValorReferencia = "1 salário mínimo por mês"
            },
            new Beneficio
            {
                IdBeneficio = 4,
                Codigo = "VALORES_A_RECEBER",
                Nome = "Valores a Receber (BCB)",
                OrgaoResponsavel = "Banco Central",
                DescricaoSimples = "Dinheiro esquecido em bancos, como sobras de contas encerradas. Qualquer pessoa pode consultar de graça.",
                LinkOficial = "https://valoresareceber.bcb.gov.br",
                ValorReferencia = "Dinheiro esquecido em bancos"
            },
            new Beneficio
            {
                IdBeneficio = 5,
                Codigo = "ABONO_SALARIAL",
                Nome = "Abono salarial PIS/Pasep",
                OrgaoResponsavel = "Ministério do Trabalho e Emprego",
                DescricaoSimples = "Valor pago uma vez por ano para quem trabalhou com carteira assinada e ganhou até dois salários mínimos por mês.",
                LinkOficial = "https://www.gov.br/trabalho-e-emprego",
                ValorReferencia = "Até 1 salário mínimo por ano"
            }
        };

        public static readonly RequisitoBeneficio[] Requisitos =
        {
            new RequisitoBeneficio { IdRequisito = 1, IdBeneficio = 1, Grupo = 1, Criterio = CriterioRequisito.Cadunico, Operador = OperadorRequisito.Igual, Valor = "S" },
            new RequisitoBeneficio { IdRequisito = 2, IdBeneficio = 1, Grupo = 1, Criterio = CriterioRequisito.RendaPcMax, Operador = OperadorRequisito.MenorOuIgual, Valor = FaixaRenda.AteMeio },
            new RequisitoBeneficio { IdRequisito = 3, IdBeneficio = 1, Grupo = 2, Criterio = CriterioRequisito.Bpc, Operador = OperadorRequisito.Igual, Valor = "S" },
            new RequisitoBeneficio { IdRequisito = 4, IdBeneficio = 2, Grupo = 1, Criterio = CriterioRequisito.IdadeMin, Operador = OperadorRequisito.MaiorOuIgual, Valor = "65" },
            new RequisitoBeneficio { IdRequisito = 5, IdBeneficio = 2, Grupo = 1, Criterio = CriterioRequisito.RendaPcMax, Operador = OperadorRequisito.MenorOuIgual, Valor = FaixaRenda.AteUmQuarto },
            new RequisitoBeneficio { IdRequisito = 6, IdBeneficio = 2, Grupo = 1, Criterio = CriterioRequisito.Cadunico, Operador = OperadorRequisito.Igual, Valor = "S" },
            new RequisitoBeneficio { IdRequisito = 7, IdBeneficio = 3, Grupo = 1, Criterio = CriterioRequisito.Deficiencia, Operador = OperadorRequisito.Igual, Valor = "S" },
            new RequisitoBeneficio { IdRequisito = 8, IdBeneficio = 3, Grupo = 1, Criterio = CriterioRequisito.RendaPcMax, Operador = OperadorRequisito.MenorOuIgual, Valor = FaixaRenda.AteUmQuarto },
            new RequisitoBeneficio { IdRequisito = 9, IdBeneficio = 3, Grupo = 1, Criterio = CriterioRequisito.Cadunico, Operador = OperadorRequisito.Igual, Valor = "S" },
            new RequisitoBeneficio { IdRequisito = 10, IdBeneficio = 5, Grupo = 1, Criterio = CriterioRequisito.VinculoFormal, Operador = OperadorRequisito.Igual, Valor = "S" }
        };

        public static readonly PassoBeneficio[] Passos =
        {
            new PassoBeneficio { IdPasso = 1, IdBeneficio = 1, Ordem = 1, Canal = CanalPasso.Presencial,
                Instrucao = "Confira se você está no CadÚnico. Se não estiver, procure o CRAS da sua cidade com documento e comprovante de endereço." },
            new PassoBeneficio { IdPasso = 2, IdBeneficio = 1, Ordem = 2, Canal = CanalPasso.Telefone,
                Instrucao = "Ligue para a empresa de energia da sua cidade e peça a Tarifa Social. Tenha em mãos o número do NIS e uma conta de luz." },
            new PassoBeneficio { IdPasso = 3, IdBeneficio = 1, Ordem = 3, Canal = CanalPasso.Site,
                Instrucao = "Confira na próxima conta de luz se o desconto da Tarifa Social apareceu." },
            new PassoBeneficio { IdPasso = 4, IdBeneficio = 2, Ordem = 1, Canal = CanalPasso.Presencial,
                Instrucao = "Deixe o CadÚnico atualizado no CRAS. Ele precisa ter sido atualizado nos últimos 2 anos." },
            new PassoBeneficio { IdPasso = 5, IdBeneficio = 2, Ordem = 2, Canal = CanalPasso.App,
                Instrucao = "Peça o BPC pelo aplicativo Meu INSS ou ligue 135." },
            new PassoBeneficio { IdPasso = 6, IdBeneficio = 2, Ordem = 3, Canal = CanalPasso.App,
                Instrucao = "Acompanhe o pedido no Meu INSS. O INSS nunca pede selfie, senha ou pagamento por WhatsApp." },
            new PassoBeneficio { IdPasso = 7, IdBeneficio = 3, Ordem = 1, Canal = CanalPasso.Presencial,
                Instrucao = "Deixe o CadÚnico atualizado no CRAS da sua cidade." },
            new PassoBeneficio { IdPasso = 8, IdBeneficio = 3, Ordem = 2, Canal = CanalPasso.App,
                Instrucao = "Peça o BPC pelo aplicativo Meu INSS ou ligue 135. Separe laudos e exames médicos." },
            new PassoBeneficio { IdPasso = 9, IdBeneficio = 3, Ordem = 3, Canal = CanalPasso.Presencial,
                Instrucao = "Vá à avaliação médica e social marcada pelo INSS." },
            new PassoBeneficio { IdPasso = 10, IdBeneficio = 4, Ordem = 1, Canal = CanalPasso.Site,
                Instrucao = "Entre no site oficial valoresareceber.bcb.gov.br e informe seu CPF e data de nascimento." },
            new PassoBeneficio { IdPasso = 11, IdBeneficio = 4, Ordem = 2, Canal = CanalPasso.Site,
                Instrucao = "Se tiver dinheiro a receber, entre com a conta gov.br e peça a devolução por Pix." },
            new PassoBeneficio { IdPasso = 12, IdBeneficio = 4, Ordem = 3, Canal = CanalPasso.Site,
                Instrucao = "Desconfie de quem cobra para liberar o dinheiro. A consulta e o resgate são gratuitos." },
            new PassoBeneficio { IdPasso = 13, IdBeneficio = 5, Ordem = 1, Canal = CanalPasso.App,
                Instrucao = "Abra o aplicativo Carteira de Trabalho Digital e veja se você tem abono a receber." },
            new PassoBeneficio { IdPasso = 14, IdBeneficio = 5, Ordem = 2, Canal = CanalPasso.App,
                Instrucao = "Confira no aplicativo a data e o banco do pagamento." },
            new PassoBeneficio { IdPasso = 15, IdBeneficio = 5, Ordem = 3, Canal = CanalPasso.Telefone,
                Instrucao = "Se tiver dúvida, ligue 158, o Alô Trabalho do Ministério do Trabalho." }
        };
    }
}
