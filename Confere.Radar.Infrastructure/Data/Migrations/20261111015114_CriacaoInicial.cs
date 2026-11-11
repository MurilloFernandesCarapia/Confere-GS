using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Confere.Radar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CF_BENEFICIO",
                columns: table => new
                {
                    ID_BENEFICIO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CODIGO = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NOME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ORGAO_RESPONSAVEL = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DESCRICAO_SIMPLES = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    LINK_OFICIAL = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    VALOR_REFERENCIA = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ATIVO = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CF_BENEFICIO", x => x.ID_BENEFICIO);
                });

            migrationBuilder.CreateTable(
                name: "CF_IDEMPOTENCIA",
                columns: table => new
                {
                    CHAVE = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ID_USUARIO_EXTERNO = table.Column<int>(type: "int", nullable: false),
                    ENDPOINT = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    HASH_REQUISICAO = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SITUACAO = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    STATUS_HTTP = table.Column<int>(type: "int", nullable: true),
                    CORPO_RESPOSTA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DT_CRIACAO = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DT_EXPIRACAO = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CF_IDEMPOTENCIA", x => x.CHAVE);
                    table.CheckConstraint("CK_IDEMPOTENCIA_SITUACAO", "[SITUACAO] IN ('PROCESSANDO','CONCLUIDA')");
                });

            migrationBuilder.CreateTable(
                name: "CF_IDENTIFICADOR_SUSPEITO",
                columns: table => new
                {
                    ID_IDENTIFICADOR = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TIPO = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    VALOR_HASH = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    VALOR_MASCARADO = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TOTAL_DENUNCIAS = table.Column<int>(type: "int", nullable: false),
                    SCORE = table.Column<int>(type: "int", nullable: false),
                    STATUS = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    DT_PRIMEIRA = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DT_ULTIMA = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CF_IDENTIFICADOR_SUSPEITO", x => x.ID_IDENTIFICADOR);
                    table.CheckConstraint("CK_IDENTIFICADOR_SCORE", "[SCORE] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_IDENTIFICADOR_STATUS", "[STATUS] IN ('EM_ANALISE','CONFIRMADO','DESCARTADO')");
                    table.CheckConstraint("CK_IDENTIFICADOR_TIPO", "[TIPO] IN ('TELEFONE','LINK','CHAVE_PIX','EMAIL','PERFIL')");
                });

            migrationBuilder.CreateTable(
                name: "CF_MUNICIPIO",
                columns: table => new
                {
                    ID_MUNICIPIO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NOME = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    UF = table.Column<string>(type: "nchar(2)", fixedLength: true, maxLength: 2, nullable: false),
                    COD_IBGE = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CF_MUNICIPIO", x => x.ID_MUNICIPIO);
                });

            migrationBuilder.CreateTable(
                name: "CF_TIPO_GOLPE",
                columns: table => new
                {
                    ID_TIPO_GOLPE = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CODIGO = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NOME = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DESCRICAO = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GRAVIDADE = table.Column<int>(type: "int", nullable: false),
                    ORGAO_IMITADO = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CF_TIPO_GOLPE", x => x.ID_TIPO_GOLPE);
                    table.CheckConstraint("CK_TIPO_GOLPE_GRAVIDADE", "[GRAVIDADE] BETWEEN 1 AND 5");
                });

            migrationBuilder.CreateTable(
                name: "CF_ELEGIBILIDADE",
                columns: table => new
                {
                    ID_ELEGIBILIDADE = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_USUARIO_EXTERNO = table.Column<int>(type: "int", nullable: false),
                    ID_BENEFICIO = table.Column<int>(type: "int", nullable: false),
                    STATUS = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    MOTIVO = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ORIGEM = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    DT_AVALIACAO = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DT_ATUALIZACAO = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CF_ELEGIBILIDADE", x => x.ID_ELEGIBILIDADE);
                    table.CheckConstraint("CK_ELEGIBILIDADE_ORIGEM", "[ORIGEM] IN ('QUESTIONARIO','DOCUMENTO_IA')");
                    table.CheckConstraint("CK_ELEGIBILIDADE_STATUS", "[STATUS] IN ('PROVAVEL','POSSIVEL','INELEGIVEL','SOLICITADO','CONCEDIDO')");
                    table.ForeignKey(
                        name: "FK_CF_ELEGIBILIDADE_CF_BENEFICIO_ID_BENEFICIO",
                        column: x => x.ID_BENEFICIO,
                        principalTable: "CF_BENEFICIO",
                        principalColumn: "ID_BENEFICIO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CF_PASSO_BENEFICIO",
                columns: table => new
                {
                    ID_PASSO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_BENEFICIO = table.Column<int>(type: "int", nullable: false),
                    ORDEM = table.Column<int>(type: "int", nullable: false),
                    INSTRUCAO = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    CANAL = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CF_PASSO_BENEFICIO", x => x.ID_PASSO);
                    table.CheckConstraint("CK_PASSO_CANAL", "[CANAL] IN ('APP','SITE','TELEFONE','PRESENCIAL')");
                    table.ForeignKey(
                        name: "FK_CF_PASSO_BENEFICIO_CF_BENEFICIO_ID_BENEFICIO",
                        column: x => x.ID_BENEFICIO,
                        principalTable: "CF_BENEFICIO",
                        principalColumn: "ID_BENEFICIO",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CF_REQUISITO_BENEFICIO",
                columns: table => new
                {
                    ID_REQUISITO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_BENEFICIO = table.Column<int>(type: "int", nullable: false),
                    GRUPO = table.Column<int>(type: "int", nullable: false),
                    CRITERIO = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OPERADOR = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    VALOR = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CF_REQUISITO_BENEFICIO", x => x.ID_REQUISITO);
                    table.CheckConstraint("CK_REQUISITO_CRITERIO", "[CRITERIO] IN ('IDADE_MIN','RENDA_PC_MAX','CADUNICO','BPC','DEFICIENCIA','VINCULO_FORMAL')");
                    table.CheckConstraint("CK_REQUISITO_OPERADOR", "[OPERADOR] IN ('>=','<=','=')");
                    table.ForeignKey(
                        name: "FK_CF_REQUISITO_BENEFICIO_CF_BENEFICIO_ID_BENEFICIO",
                        column: x => x.ID_BENEFICIO,
                        principalTable: "CF_BENEFICIO",
                        principalColumn: "ID_BENEFICIO",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CF_ORGAO_PARCEIRO",
                columns: table => new
                {
                    ID_ORGAO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NOME = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TIPO = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    ID_MUNICIPIO = table.Column<int>(type: "int", nullable: false),
                    EMAIL_CONTATO = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CF_ORGAO_PARCEIRO", x => x.ID_ORGAO);
                    table.CheckConstraint("CK_ORGAO_TIPO", "[TIPO] IN ('CRAS','PROCON','PREFEITURA','BANCO','COOPERATIVA','ONG')");
                    table.ForeignKey(
                        name: "FK_CF_ORGAO_PARCEIRO_CF_MUNICIPIO_ID_MUNICIPIO",
                        column: x => x.ID_MUNICIPIO,
                        principalTable: "CF_MUNICIPIO",
                        principalColumn: "ID_MUNICIPIO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CF_PERFIL_SOCIAL",
                columns: table => new
                {
                    ID_USUARIO_EXTERNO = table.Column<int>(type: "int", nullable: false),
                    IDADE = table.Column<int>(type: "int", nullable: true),
                    RENDA_PC_FAIXA = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    QTD_MORADORES = table.Column<int>(type: "int", nullable: true),
                    INSCRITO_CADUNICO = table.Column<bool>(type: "bit", nullable: true),
                    RECEBE_BPC = table.Column<bool>(type: "bit", nullable: true),
                    DEFICIENCIA = table.Column<bool>(type: "bit", nullable: true),
                    VINCULO_FORMAL = table.Column<bool>(type: "bit", nullable: true),
                    ID_MUNICIPIO = table.Column<int>(type: "int", nullable: true),
                    DT_CONSENTIMENTO = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DT_ATUALIZACAO = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CF_PERFIL_SOCIAL", x => x.ID_USUARIO_EXTERNO);
                    table.CheckConstraint("CK_PERFIL_RENDA", "[RENDA_PC_FAIXA] IS NULL OR [RENDA_PC_FAIXA] IN ('ATE_1_4','ATE_1_2','ATE_1','ATE_2','ACIMA_2')");
                    table.ForeignKey(
                        name: "FK_CF_PERFIL_SOCIAL_CF_MUNICIPIO_ID_MUNICIPIO",
                        column: x => x.ID_MUNICIPIO,
                        principalTable: "CF_MUNICIPIO",
                        principalColumn: "ID_MUNICIPIO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CF_DENUNCIA",
                columns: table => new
                {
                    ID_DENUNCIA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_USUARIO_EXTERNO = table.Column<int>(type: "int", nullable: false),
                    ID_IDENTIFICADOR = table.Column<int>(type: "int", nullable: false),
                    ID_TIPO_GOLPE = table.Column<int>(type: "int", nullable: false),
                    ID_MUNICIPIO = table.Column<int>(type: "int", nullable: false),
                    ID_VERIFICACAO_EXTERNA = table.Column<int>(type: "int", nullable: true),
                    DESCRICAO = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DT_DENUNCIA = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DT_ATUALIZACAO = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CF_DENUNCIA", x => x.ID_DENUNCIA);
                    table.ForeignKey(
                        name: "FK_CF_DENUNCIA_CF_IDENTIFICADOR_SUSPEITO_ID_IDENTIFICADOR",
                        column: x => x.ID_IDENTIFICADOR,
                        principalTable: "CF_IDENTIFICADOR_SUSPEITO",
                        principalColumn: "ID_IDENTIFICADOR",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CF_DENUNCIA_CF_MUNICIPIO_ID_MUNICIPIO",
                        column: x => x.ID_MUNICIPIO,
                        principalTable: "CF_MUNICIPIO",
                        principalColumn: "ID_MUNICIPIO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CF_DENUNCIA_CF_TIPO_GOLPE_ID_TIPO_GOLPE",
                        column: x => x.ID_TIPO_GOLPE,
                        principalTable: "CF_TIPO_GOLPE",
                        principalColumn: "ID_TIPO_GOLPE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CF_GESTOR",
                columns: table => new
                {
                    ID_GESTOR = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_ORGAO = table.Column<int>(type: "int", nullable: false),
                    NOME = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CARGO = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    EMAIL = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SENHA_HASH = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ATIVO = table.Column<bool>(type: "bit", nullable: false),
                    DT_CADASTRO = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CF_GESTOR", x => x.ID_GESTOR);
                    table.ForeignKey(
                        name: "FK_CF_GESTOR_CF_ORGAO_PARCEIRO_ID_ORGAO",
                        column: x => x.ID_ORGAO,
                        principalTable: "CF_ORGAO_PARCEIRO",
                        principalColumn: "ID_ORGAO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CF_ALERTA_REGIONAL",
                columns: table => new
                {
                    ID_ALERTA_REGIONAL = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_MUNICIPIO = table.Column<int>(type: "int", nullable: false),
                    ID_TIPO_GOLPE = table.Column<int>(type: "int", nullable: false),
                    ID_GESTOR = table.Column<int>(type: "int", nullable: true),
                    TITULO = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    MENSAGEM = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    NIVEL = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    ORIGEM = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DT_INICIO = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DT_FIM = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ATIVO = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CF_ALERTA_REGIONAL", x => x.ID_ALERTA_REGIONAL);
                    table.CheckConstraint("CK_ALERTA_NIVEL", "[NIVEL] IN ('INFO','ATENCAO','URGENTE')");
                    table.CheckConstraint("CK_ALERTA_ORIGEM", "[ORIGEM] IN ('AUTOMATICO','GESTOR')");
                    table.CheckConstraint("CK_ALERTA_PERIODO", "[DT_FIM] > [DT_INICIO]");
                    table.ForeignKey(
                        name: "FK_CF_ALERTA_REGIONAL_CF_GESTOR_ID_GESTOR",
                        column: x => x.ID_GESTOR,
                        principalTable: "CF_GESTOR",
                        principalColumn: "ID_GESTOR",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CF_ALERTA_REGIONAL_CF_MUNICIPIO_ID_MUNICIPIO",
                        column: x => x.ID_MUNICIPIO,
                        principalTable: "CF_MUNICIPIO",
                        principalColumn: "ID_MUNICIPIO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CF_ALERTA_REGIONAL_CF_TIPO_GOLPE_ID_TIPO_GOLPE",
                        column: x => x.ID_TIPO_GOLPE,
                        principalTable: "CF_TIPO_GOLPE",
                        principalColumn: "ID_TIPO_GOLPE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CF_BENEFICIO",
                columns: new[] { "ID_BENEFICIO", "ATIVO", "CODIGO", "DESCRICAO_SIMPLES", "LINK_OFICIAL", "NOME", "ORGAO_RESPONSAVEL", "VALOR_REFERENCIA" },
                values: new object[,]
                {
                    { 1, true, "TARIFA_SOCIAL", "Desconto na conta de luz para famílias de baixa renda inscritas no CadÚnico ou que recebem BPC.", "https://www.gov.br/aneel", "Tarifa Social de Energia", "ANEEL e distribuidora de energia", "Até 65% de desconto na conta" },
                    { 2, true, "BPC_IDOSO", "Um salário mínimo por mês para quem tem 65 anos ou mais e renda familiar baixa. Não precisa ter contribuído com o INSS.", "https://www.gov.br/inss", "BPC para pessoa idosa", "INSS", "1 salário mínimo por mês" },
                    { 3, true, "BPC_PCD", "Um salário mínimo por mês para pessoas com deficiência de qualquer idade e renda familiar baixa.", "https://www.gov.br/inss", "BPC para pessoa com deficiência", "INSS", "1 salário mínimo por mês" },
                    { 4, true, "VALORES_A_RECEBER", "Dinheiro esquecido em bancos, como sobras de contas encerradas. Qualquer pessoa pode consultar de graça.", "https://valoresareceber.bcb.gov.br", "Valores a Receber (BCB)", "Banco Central", "Dinheiro esquecido em bancos" },
                    { 5, true, "ABONO_SALARIAL", "Valor pago uma vez por ano para quem trabalhou com carteira assinada e ganhou até dois salários mínimos por mês.", "https://www.gov.br/trabalho-e-emprego", "Abono salarial PIS/Pasep", "Ministério do Trabalho e Emprego", "Até 1 salário mínimo por ano" }
                });

            migrationBuilder.InsertData(
                table: "CF_MUNICIPIO",
                columns: new[] { "ID_MUNICIPIO", "COD_IBGE", "NOME", "UF" },
                values: new object[,]
                {
                    { 1, 3550308, "São Paulo", "SP" },
                    { 2, 3518800, "Guarulhos", "SP" },
                    { 3, 3509502, "Campinas", "SP" },
                    { 4, 3534401, "Osasco", "SP" },
                    { 5, 3547809, "Santo André", "SP" },
                    { 6, 3304557, "Rio de Janeiro", "RJ" },
                    { 7, 3106200, "Belo Horizonte", "MG" },
                    { 8, 4106902, "Curitiba", "PR" },
                    { 9, 4314902, "Porto Alegre", "RS" },
                    { 10, 2927408, "Salvador", "BA" },
                    { 11, 2611606, "Recife", "PE" },
                    { 12, 2304400, "Fortaleza", "CE" },
                    { 13, 5300108, "Brasília", "DF" }
                });

            migrationBuilder.InsertData(
                table: "CF_TIPO_GOLPE",
                columns: new[] { "ID_TIPO_GOLPE", "CODIGO", "DESCRICAO", "GRAVIDADE", "NOME", "ORGAO_IMITADO" },
                values: new object[,]
                {
                    { 1, "FALSA_PROVA_VIDA", "Pede selfie ou foto de documento dizendo que o benefício vai ser bloqueado. O INSS não faz prova de vida por WhatsApp.", 5, "Falsa prova de vida", "INSS" },
                    { 2, "FALSO_REEMBOLSO_INSS", "Promete devolver descontos indevidos do INSS e pede dados ou pagamento de taxa.", 5, "Falsa devolução de descontos do INSS", "INSS" },
                    { 3, "FALSA_CENTRAL", "Liga dizendo ser do banco, fala de compra suspeita e pede senha, código ou transferência.", 5, "Falsa central do banco", "Bancos" },
                    { 4, "FALSO_PARENTE", "Alguém finge ser um parente com número novo e pede dinheiro com urgência.", 4, "\"Oi mãe, troquei de número\"", "Família" },
                    { 5, "FALSO_VALORES_RECEBER", "Diz que você tem dinheiro esquecido e cobra taxa para liberar. O resgate oficial é gratuito.", 4, "Falso resgate de valores esquecidos", "Banco Central" },
                    { 6, "FALSO_EMPRESTIMO", "Oferece empréstimo com condições boas demais ou contrata sem autorização.", 4, "Empréstimo consignado falso ou não autorizado", "Bancos / INSS" },
                    { 7, "DEEPFAKE_CELEBRIDADE", "Vídeo feito com IA de alguém conhecido prometendo dinheiro ou benefício.", 4, "Vídeo falso de jornalista ou famoso", "Imprensa" },
                    { 8, "FALSO_BOLETO", "Boleto com dados trocados para o dinheiro ir para outra pessoa.", 3, "Boleto ou conta adulterada", "Concessionárias" },
                    { 9, "LINK_RASTREIO", "Mensagem com link cobrando taxa para liberar uma encomenda.", 3, "Falsa taxa de entrega ou rastreio", "Correios" },
                    { 10, "PIX_ERRADO", "Pede para devolver um Pix que nunca foi feito ou que vai ser estornado depois.", 3, "\"Te mandei um Pix por engano\"", null }
                });

            migrationBuilder.InsertData(
                table: "CF_ORGAO_PARCEIRO",
                columns: new[] { "ID_ORGAO", "EMAIL_CONTATO", "ID_MUNICIPIO", "NOME", "TIPO" },
                values: new object[] { 1, null, 1, "CRAS Centro - São Paulo", "CRAS" });

            migrationBuilder.InsertData(
                table: "CF_PASSO_BENEFICIO",
                columns: new[] { "ID_PASSO", "CANAL", "ID_BENEFICIO", "INSTRUCAO", "ORDEM" },
                values: new object[,]
                {
                    { 1, "PRESENCIAL", 1, "Confira se você está no CadÚnico. Se não estiver, procure o CRAS da sua cidade com documento e comprovante de endereço.", 1 },
                    { 2, "TELEFONE", 1, "Ligue para a empresa de energia da sua cidade e peça a Tarifa Social. Tenha em mãos o número do NIS e uma conta de luz.", 2 },
                    { 3, "SITE", 1, "Confira na próxima conta de luz se o desconto da Tarifa Social apareceu.", 3 },
                    { 4, "PRESENCIAL", 2, "Deixe o CadÚnico atualizado no CRAS. Ele precisa ter sido atualizado nos últimos 2 anos.", 1 },
                    { 5, "APP", 2, "Peça o BPC pelo aplicativo Meu INSS ou ligue 135.", 2 },
                    { 6, "APP", 2, "Acompanhe o pedido no Meu INSS. O INSS nunca pede selfie, senha ou pagamento por WhatsApp.", 3 },
                    { 7, "PRESENCIAL", 3, "Deixe o CadÚnico atualizado no CRAS da sua cidade.", 1 },
                    { 8, "APP", 3, "Peça o BPC pelo aplicativo Meu INSS ou ligue 135. Separe laudos e exames médicos.", 2 },
                    { 9, "PRESENCIAL", 3, "Vá à avaliação médica e social marcada pelo INSS.", 3 },
                    { 10, "SITE", 4, "Entre no site oficial valoresareceber.bcb.gov.br e informe seu CPF e data de nascimento.", 1 },
                    { 11, "SITE", 4, "Se tiver dinheiro a receber, entre com a conta gov.br e peça a devolução por Pix.", 2 },
                    { 12, "SITE", 4, "Desconfie de quem cobra para liberar o dinheiro. A consulta e o resgate são gratuitos.", 3 },
                    { 13, "APP", 5, "Abra o aplicativo Carteira de Trabalho Digital e veja se você tem abono a receber.", 1 },
                    { 14, "APP", 5, "Confira no aplicativo a data e o banco do pagamento.", 2 },
                    { 15, "TELEFONE", 5, "Se tiver dúvida, ligue 158, o Alô Trabalho do Ministério do Trabalho.", 3 }
                });

            migrationBuilder.InsertData(
                table: "CF_REQUISITO_BENEFICIO",
                columns: new[] { "ID_REQUISITO", "CRITERIO", "GRUPO", "ID_BENEFICIO", "OPERADOR", "VALOR" },
                values: new object[,]
                {
                    { 1, "CADUNICO", 1, 1, "=", "S" },
                    { 2, "RENDA_PC_MAX", 1, 1, "<=", "ATE_1_2" },
                    { 3, "BPC", 2, 1, "=", "S" },
                    { 4, "IDADE_MIN", 1, 2, ">=", "65" },
                    { 5, "RENDA_PC_MAX", 1, 2, "<=", "ATE_1_4" },
                    { 6, "CADUNICO", 1, 2, "=", "S" },
                    { 7, "DEFICIENCIA", 1, 3, "=", "S" },
                    { 8, "RENDA_PC_MAX", 1, 3, "<=", "ATE_1_4" },
                    { 9, "CADUNICO", 1, 3, "=", "S" },
                    { 10, "VINCULO_FORMAL", 1, 5, "=", "S" }
                });

            migrationBuilder.InsertData(
                table: "CF_GESTOR",
                columns: new[] { "ID_GESTOR", "ATIVO", "CARGO", "DT_CADASTRO", "EMAIL", "ID_ORGAO", "NOME", "SENHA_HASH" },
                values: new object[] { 1, true, "Coordenador", new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "gestor@cras.sp.gov.br", 1, "Gestor de Teste CRAS", "$2a$11$lckz4ODK.px//fnKXwwHuuE0ddFgWQH09d6bTCdx23To3ppoHePAy" });

            migrationBuilder.CreateIndex(
                name: "IX_CF_ALERTA_REGIONAL_ID_GESTOR",
                table: "CF_ALERTA_REGIONAL",
                column: "ID_GESTOR");

            migrationBuilder.CreateIndex(
                name: "IX_CF_ALERTA_REGIONAL_ID_MUNICIPIO_ATIVO_DT_FIM",
                table: "CF_ALERTA_REGIONAL",
                columns: new[] { "ID_MUNICIPIO", "ATIVO", "DT_FIM" });

            migrationBuilder.CreateIndex(
                name: "IX_CF_ALERTA_REGIONAL_ID_TIPO_GOLPE",
                table: "CF_ALERTA_REGIONAL",
                column: "ID_TIPO_GOLPE");

            migrationBuilder.CreateIndex(
                name: "IX_CF_BENEFICIO_CODIGO",
                table: "CF_BENEFICIO",
                column: "CODIGO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CF_DENUNCIA_ID_IDENTIFICADOR",
                table: "CF_DENUNCIA",
                column: "ID_IDENTIFICADOR");

            migrationBuilder.CreateIndex(
                name: "IX_CF_DENUNCIA_ID_MUNICIPIO_DT_DENUNCIA",
                table: "CF_DENUNCIA",
                columns: new[] { "ID_MUNICIPIO", "DT_DENUNCIA" });

            migrationBuilder.CreateIndex(
                name: "IX_CF_DENUNCIA_ID_TIPO_GOLPE",
                table: "CF_DENUNCIA",
                column: "ID_TIPO_GOLPE");

            migrationBuilder.CreateIndex(
                name: "IX_CF_DENUNCIA_ID_USUARIO_EXTERNO_ID_IDENTIFICADOR_DT_DENUNCIA",
                table: "CF_DENUNCIA",
                columns: new[] { "ID_USUARIO_EXTERNO", "ID_IDENTIFICADOR", "DT_DENUNCIA" });

            migrationBuilder.CreateIndex(
                name: "IX_CF_ELEGIBILIDADE_ID_BENEFICIO",
                table: "CF_ELEGIBILIDADE",
                column: "ID_BENEFICIO");

            migrationBuilder.CreateIndex(
                name: "IX_CF_ELEGIBILIDADE_ID_USUARIO_EXTERNO_ID_BENEFICIO",
                table: "CF_ELEGIBILIDADE",
                columns: new[] { "ID_USUARIO_EXTERNO", "ID_BENEFICIO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CF_GESTOR_EMAIL",
                table: "CF_GESTOR",
                column: "EMAIL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CF_GESTOR_ID_ORGAO",
                table: "CF_GESTOR",
                column: "ID_ORGAO");

            migrationBuilder.CreateIndex(
                name: "IX_CF_IDEMPOTENCIA_DT_EXPIRACAO",
                table: "CF_IDEMPOTENCIA",
                column: "DT_EXPIRACAO");

            migrationBuilder.CreateIndex(
                name: "IX_CF_IDENTIFICADOR_SUSPEITO_TIPO_VALOR_HASH",
                table: "CF_IDENTIFICADOR_SUSPEITO",
                columns: new[] { "TIPO", "VALOR_HASH" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CF_MUNICIPIO_COD_IBGE",
                table: "CF_MUNICIPIO",
                column: "COD_IBGE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CF_ORGAO_PARCEIRO_ID_MUNICIPIO",
                table: "CF_ORGAO_PARCEIRO",
                column: "ID_MUNICIPIO");

            migrationBuilder.CreateIndex(
                name: "IX_CF_PASSO_BENEFICIO_ID_BENEFICIO_ORDEM",
                table: "CF_PASSO_BENEFICIO",
                columns: new[] { "ID_BENEFICIO", "ORDEM" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CF_PERFIL_SOCIAL_ID_MUNICIPIO",
                table: "CF_PERFIL_SOCIAL",
                column: "ID_MUNICIPIO");

            migrationBuilder.CreateIndex(
                name: "IX_CF_REQUISITO_BENEFICIO_ID_BENEFICIO",
                table: "CF_REQUISITO_BENEFICIO",
                column: "ID_BENEFICIO");

            migrationBuilder.CreateIndex(
                name: "IX_CF_TIPO_GOLPE_CODIGO",
                table: "CF_TIPO_GOLPE",
                column: "CODIGO",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CF_ALERTA_REGIONAL");

            migrationBuilder.DropTable(
                name: "CF_DENUNCIA");

            migrationBuilder.DropTable(
                name: "CF_ELEGIBILIDADE");

            migrationBuilder.DropTable(
                name: "CF_IDEMPOTENCIA");

            migrationBuilder.DropTable(
                name: "CF_PASSO_BENEFICIO");

            migrationBuilder.DropTable(
                name: "CF_PERFIL_SOCIAL");

            migrationBuilder.DropTable(
                name: "CF_REQUISITO_BENEFICIO");

            migrationBuilder.DropTable(
                name: "CF_GESTOR");

            migrationBuilder.DropTable(
                name: "CF_IDENTIFICADOR_SUSPEITO");

            migrationBuilder.DropTable(
                name: "CF_TIPO_GOLPE");

            migrationBuilder.DropTable(
                name: "CF_BENEFICIO");

            migrationBuilder.DropTable(
                name: "CF_ORGAO_PARCEIRO");

            migrationBuilder.DropTable(
                name: "CF_MUNICIPIO");
        }
    }
}
