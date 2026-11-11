using Confere.Radar.Application.Common;
using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Dtos.Direitos;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Confere.Radar.Application.Services
{
    public class ElegibilidadeService : IElegibilidadeService
    {
        private const int TamanhoMaximoMotivo = 300;

        private readonly IElegibilidadeRepository _elegibilidadeRepository;
        private readonly IPerfilSocialRepository _perfilSocialRepository;
        private readonly IBeneficioRepository _beneficioRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<ElegibilidadeService> _logger;

        public ElegibilidadeService(
            IElegibilidadeRepository elegibilidadeRepository,
            IPerfilSocialRepository perfilSocialRepository,
            IBeneficioRepository beneficioRepository,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider,
            ILogger<ElegibilidadeService> logger)
        {
            _elegibilidadeRepository = elegibilidadeRepository;
            _perfilSocialRepository = perfilSocialRepository;
            _beneficioRepository = beneficioRepository;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public ResultadoElegibilidade AvaliarBeneficio(Beneficio beneficio, PerfilSocial perfil)
        {
            var grupos = beneficio.Requisitos
                .GroupBy(r => r.Grupo)
                .OrderBy(g => g.Key)
                .ToList();

            if (grupos.Count == 0)
            {
                return new ResultadoElegibilidade(StatusElegibilidade.Possivel,
                    "Esse benefício não depende do seu perfil social. Vale conferir no canal oficial se existe algo para você.");
            }

            var dadosFaltantes = new List<string>();
            var requisitosNaoAtendidos = new List<string>();

            foreach (var grupo in grupos)
            {
                var avaliacoes = grupo.Select(r => (Requisito: r, Atende: Atende(r, perfil))).ToList();

                if (avaliacoes.All(a => a.Atende == true))
                {
                    var frases = avaliacoes.Select(a => FraseAtendida(a.Requisito));
                    return new ResultadoElegibilidade(StatusElegibilidade.Provavel, Limitar($"Você {Juntar(frases)}."));
                }

                if (avaliacoes.Any(a => a.Atende == false))
                {
                    requisitosNaoAtendidos.AddRange(avaliacoes
                        .Where(a => a.Atende == false)
                        .Select(a => PrimeiraMinuscula(DescricaoRequisito.Descrever(a.Requisito))));
                }
                else
                {
                    dadosFaltantes.AddRange(avaliacoes
                        .Where(a => a.Atende is null)
                        .Select(a => NomeDoDado(a.Requisito.Criterio)));
                }
            }

            if (dadosFaltantes.Count > 0)
            {
                return new ResultadoElegibilidade(StatusElegibilidade.Possivel,
                    Limitar($"Faltam dados no seu perfil para ter certeza: {Juntar(dadosFaltantes.Distinct())}. Complete o perfil social e avalie de novo."));
            }

            return new ResultadoElegibilidade(StatusElegibilidade.Inelegivel,
                Limitar($"Pelo que você informou, ainda não dá para pedir. Requisito não atendido: {Juntar(requisitosNaoAtendidos.Distinct())}."));
        }

        public async Task<IReadOnlyList<ElegibilidadeResponse>> AvaliarAsync(int idUsuario)
        {
            var perfil = await _perfilSocialRepository.ObterAsync(idUsuario)
                ?? throw new ConsentimentoPendenteException("Preencha o perfil social em PUT /perfil-social, com consentimento, antes de avaliar seus direitos.");

            var beneficios = await _beneficioRepository.ListarAtivosAsync();
            var existentes = (await _elegibilidadeRepository.ListarDoUsuarioAsync(idUsuario)).ToDictionary(e => e.IdBeneficio);
            var agora = _timeProvider.GetUtcNow().UtcDateTime;
            var avaliadas = new List<(Elegibilidade Elegibilidade, Beneficio Beneficio)>();

            foreach (var beneficio in beneficios)
            {
                if (!existentes.TryGetValue(beneficio.IdBeneficio, out var elegibilidade))
                {
                    elegibilidade = new Elegibilidade
                    {
                        IdUsuario = idUsuario,
                        IdBeneficio = beneficio.IdBeneficio
                    };

                    await _elegibilidadeRepository.AdicionarAsync(elegibilidade);
                }
                else if (StatusElegibilidade.MarcadoPeloCidadao(elegibilidade.Status))
                {
                    avaliadas.Add((elegibilidade, beneficio));
                    continue;
                }

                var resultado = AvaliarBeneficio(beneficio, perfil);

                elegibilidade.Status = resultado.Status;
                elegibilidade.Motivo = resultado.Motivo;
                elegibilidade.Origem = OrigemElegibilidade.Questionario;
                elegibilidade.DtAvaliacao = agora;
                elegibilidade.DtAtualizacao = agora;

                avaliadas.Add((elegibilidade, beneficio));
            }

            await _unitOfWork.SalvarAsync();

            _logger.LogInformation("Elegibilidade do usuário {IdUsuario} avaliada para {Quantidade} benefícios", idUsuario, avaliadas.Count);

            return Ordenar(avaliadas.Select(a => ParaResposta(a.Elegibilidade, a.Beneficio)));
        }

        public async Task<IReadOnlyList<ElegibilidadeResponse>> ListarAsync(int idUsuario)
        {
            var elegibilidades = await _elegibilidadeRepository.ListarDoUsuarioAsync(idUsuario);
            return Ordenar(elegibilidades.Select(e => ParaResposta(e, e.Beneficio!)));
        }

        public async Task<ElegibilidadeResponse> AtualizarStatusAsync(int id, AtualizarElegibilidadeRequest request, int idUsuario)
        {
            var elegibilidade = await _elegibilidadeRepository.ObterPorIdAsync(id)
                ?? throw new NaoEncontradoException("Resultado de elegibilidade não encontrado.");

            if (elegibilidade.IdUsuario != idUsuario)
            {
                throw new SemPermissaoException("Você só pode alterar os seus próprios resultados.");
            }

            var novoStatus = request.Status.Trim().ToUpperInvariant();

            if (elegibilidade.Status == StatusElegibilidade.Concedido && novoStatus == StatusElegibilidade.Solicitado)
            {
                throw new ConflitoException("Esse benefício já está marcado como concedido.");
            }

            elegibilidade.Status = novoStatus;
            elegibilidade.Motivo = novoStatus == StatusElegibilidade.Concedido
                ? "Você marcou que o benefício foi concedido."
                : "Você marcou que já fez o pedido. Acompanhe pelo canal oficial.";
            elegibilidade.DtAtualizacao = _timeProvider.GetUtcNow().UtcDateTime;

            await _unitOfWork.SalvarAsync();

            _logger.LogInformation("Elegibilidade {IdElegibilidade} marcada como {Status} pelo usuário {IdUsuario}", id, novoStatus, idUsuario);
            return ParaResposta(elegibilidade, elegibilidade.Beneficio!);
        }

        private static bool? Atende(RequisitoBeneficio requisito, PerfilSocial perfil)
        {
            return requisito.Criterio switch
            {
                CriterioRequisito.IdadeMin => perfil.Idade is null || !int.TryParse(requisito.Valor, out var idade)
                    ? null
                    : Comparar(perfil.Idade.Value, idade, requisito.Operador),
                CriterioRequisito.RendaPcMax => perfil.RendaPcFaixa is null || FaixaRenda.Posicao(requisito.Valor) < 0
                    ? null
                    : Comparar(FaixaRenda.Posicao(perfil.RendaPcFaixa), FaixaRenda.Posicao(requisito.Valor), requisito.Operador),
                CriterioRequisito.Cadunico => CompararSimNao(perfil.InscritoCadunico, requisito.Valor),
                CriterioRequisito.Bpc => CompararSimNao(perfil.RecebeBpc, requisito.Valor),
                CriterioRequisito.Deficiencia => CompararSimNao(perfil.Deficiencia, requisito.Valor),
                CriterioRequisito.VinculoFormal => CompararSimNao(perfil.VinculoFormal, requisito.Valor),
                _ => null
            };
        }

        private static bool Comparar(int valorPerfil, int valorRequisito, string operador)
        {
            return operador switch
            {
                OperadorRequisito.MaiorOuIgual => valorPerfil >= valorRequisito,
                OperadorRequisito.MenorOuIgual => valorPerfil <= valorRequisito,
                _ => valorPerfil == valorRequisito
            };
        }

        private static bool? CompararSimNao(bool? valorPerfil, string valorRequisito)
        {
            return valorPerfil is null ? null : valorPerfil.Value == (valorRequisito == "S");
        }

        private static string FraseAtendida(RequisitoBeneficio requisito)
        {
            var negacao = requisito.Valor == "N" ? "não " : string.Empty;

            return requisito.Criterio switch
            {
                CriterioRequisito.IdadeMin => $"tem {requisito.Valor} anos ou mais",
                CriterioRequisito.RendaPcMax => $"tem renda por pessoa de {DescricaoRequisito.DescreverFaixa(requisito.Valor)}",
                CriterioRequisito.Cadunico => $"{negacao}está inscrito no CadÚnico",
                CriterioRequisito.Bpc => $"{negacao}recebe o BPC",
                CriterioRequisito.Deficiencia => $"{negacao}tem deficiência",
                CriterioRequisito.VinculoFormal => $"{negacao}trabalhou com carteira assinada",
                _ => PrimeiraMinuscula(DescricaoRequisito.Descrever(requisito))
            };
        }

        private static string NomeDoDado(string criterio)
        {
            return criterio switch
            {
                CriterioRequisito.IdadeMin => "idade",
                CriterioRequisito.RendaPcMax => "faixa de renda por pessoa",
                CriterioRequisito.Cadunico => "se tem CadÚnico",
                CriterioRequisito.Bpc => "se recebe BPC",
                CriterioRequisito.Deficiencia => "se tem deficiência",
                CriterioRequisito.VinculoFormal => "se trabalhou com carteira assinada",
                _ => criterio.ToLowerInvariant()
            };
        }

        private static IReadOnlyList<ElegibilidadeResponse> Ordenar(IEnumerable<ElegibilidadeResponse> respostas)
        {
            return respostas
                .OrderBy(r => Prioridade(r.Status))
                .ThenBy(r => r.Beneficio.Nome)
                .ToList();
        }

        private static int Prioridade(string status)
        {
            return status switch
            {
                StatusElegibilidade.Concedido => 0,
                StatusElegibilidade.Solicitado => 1,
                StatusElegibilidade.Provavel => 2,
                StatusElegibilidade.Possivel => 3,
                _ => 4
            };
        }

        private static ElegibilidadeResponse ParaResposta(Elegibilidade elegibilidade, Beneficio beneficio)
        {
            return new ElegibilidadeResponse(
                elegibilidade.IdElegibilidade,
                new BeneficioElegibilidadeResponse(beneficio.Codigo, beneficio.Nome, beneficio.ValorReferencia),
                elegibilidade.Status,
                elegibilidade.Motivo,
                elegibilidade.Origem,
                HorarioBrasilia.Converter(elegibilidade.DtAvaliacao),
                HorarioBrasilia.Converter(elegibilidade.DtAtualizacao));
        }

        private static string Juntar(IEnumerable<string> partes)
        {
            var lista = partes.ToList();

            return lista.Count switch
            {
                0 => string.Empty,
                1 => lista[0],
                _ => $"{string.Join(", ", lista.Take(lista.Count - 1))} e {lista[^1]}"
            };
        }

        private static string PrimeiraMinuscula(string texto)
        {
            return string.IsNullOrEmpty(texto) ? texto : char.ToLowerInvariant(texto[0]) + texto[1..];
        }

        private static string Limitar(string texto)
        {
            return texto.Length <= TamanhoMaximoMotivo ? texto : texto[..(TamanhoMaximoMotivo - 1)] + "…";
        }
    }
}
