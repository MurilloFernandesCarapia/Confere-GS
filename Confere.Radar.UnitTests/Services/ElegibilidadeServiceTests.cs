using Confere.Radar.Application.Services;
using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Dtos.Direitos;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.UnitTests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Confere.Radar.UnitTests.Services
{
    public class ElegibilidadeServiceTests
    {
        private readonly Mock<IElegibilidadeRepository> _elegibilidadeRepository = new();
        private readonly Mock<IPerfilSocialRepository> _perfilRepository = new();
        private readonly Mock<IBeneficioRepository> _beneficioRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly RelogioFixo _relogio = new();
        private readonly ElegibilidadeService _service;

        public ElegibilidadeServiceTests()
        {
            _service = new ElegibilidadeService(
                _elegibilidadeRepository.Object,
                _perfilRepository.Object,
                _beneficioRepository.Object,
                _unitOfWork.Object,
                _relogio,
                NullLogger<ElegibilidadeService>.Instance);
        }

        public static PerfilSocial PerfilIdosaDoContrato() => new()
        {
            IdUsuario = 1,
            Idade = 68,
            RendaPcFaixa = FaixaRenda.AteUmQuarto,
            QtdMoradores = 2,
            InscritoCadunico = true,
            RecebeBpc = false,
            Deficiencia = false,
            VinculoFormal = false
        };

        [Fact]
        public void RN16_TarifaSocial_PeloGrupoDoCadunicoERenda_Provavel()
        {
            var resultado = _service.AvaliarBeneficio(CatalogoBeneficios.TarifaSocial(), PerfilIdosaDoContrato());

            Assert.Equal(StatusElegibilidade.Provavel, resultado.Status);
            Assert.Contains("CadÚnico", resultado.Motivo);
        }

        [Fact]
        public void RN16_TarifaSocial_PeloGrupoDoBpc_ProvavelMesmoComRendaAlta()
        {
            var perfil = new PerfilSocial { RendaPcFaixa = FaixaRenda.AcimaDeDois, InscritoCadunico = false, RecebeBpc = true };

            var resultado = _service.AvaliarBeneficio(CatalogoBeneficios.TarifaSocial(), perfil);

            Assert.Equal(StatusElegibilidade.Provavel, resultado.Status);
            Assert.Equal("Você recebe o BPC.", resultado.Motivo);
        }

        [Fact]
        public void RN16_TarifaSocial_NenhumGrupoAtendido_Inelegivel()
        {
            var perfil = new PerfilSocial { RendaPcFaixa = FaixaRenda.AteDois, InscritoCadunico = true, RecebeBpc = false };

            var resultado = _service.AvaliarBeneficio(CatalogoBeneficios.TarifaSocial(), perfil);

            Assert.Equal(StatusElegibilidade.Inelegivel, resultado.Status);
        }

        [Fact]
        public void RN16_TarifaSocial_SemSaberDoBpc_EComRendaAlta_Possivel()
        {
            var perfil = new PerfilSocial { RendaPcFaixa = FaixaRenda.AteDois, InscritoCadunico = true, RecebeBpc = null };

            var resultado = _service.AvaliarBeneficio(CatalogoBeneficios.TarifaSocial(), perfil);

            Assert.Equal(StatusElegibilidade.Possivel, resultado.Status);
            Assert.Contains("se recebe BPC", resultado.Motivo);
        }

        [Fact]
        public void RN16_BpcIdoso_PerfilDoContrato_Provavel()
        {
            var resultado = _service.AvaliarBeneficio(CatalogoBeneficios.BpcIdoso(), PerfilIdosaDoContrato());

            Assert.Equal(StatusElegibilidade.Provavel, resultado.Status);
            Assert.Contains("65 anos ou mais", resultado.Motivo);
        }

        [Fact]
        public void RN16_BpcIdoso_Com60Anos_Inelegivel()
        {
            var perfil = PerfilIdosaDoContrato();
            perfil.Idade = 60;

            var resultado = _service.AvaliarBeneficio(CatalogoBeneficios.BpcIdoso(), perfil);

            Assert.Equal(StatusElegibilidade.Inelegivel, resultado.Status);
            Assert.Contains("ter 65 anos ou mais", resultado.Motivo);
        }

        [Fact]
        public void RN16_BpcIdoso_SemIdadeInformada_Possivel()
        {
            var perfil = PerfilIdosaDoContrato();
            perfil.Idade = null;

            var resultado = _service.AvaliarBeneficio(CatalogoBeneficios.BpcIdoso(), perfil);

            Assert.Equal(StatusElegibilidade.Possivel, resultado.Status);
            Assert.Contains("idade", resultado.Motivo);
        }

        [Fact]
        public void RN16_NoMesmoGrupo_RequisitoNaoAtendidoPesaMaisQueDadoFaltante()
        {
            var perfil = new PerfilSocial { Idade = null, RendaPcFaixa = FaixaRenda.AcimaDeDois, InscritoCadunico = null };

            var resultado = _service.AvaliarBeneficio(CatalogoBeneficios.BpcIdoso(), perfil);

            Assert.Equal(StatusElegibilidade.Inelegivel, resultado.Status);
        }

        [Fact]
        public void RN16_BpcPcd_SemDeficiencia_Inelegivel()
        {
            var resultado = _service.AvaliarBeneficio(CatalogoBeneficios.BpcPcd(), PerfilIdosaDoContrato());

            Assert.Equal(StatusElegibilidade.Inelegivel, resultado.Status);
        }

        [Fact]
        public void RN16_BpcPcd_ComDeficienciaRendaECadunico_Provavel()
        {
            var perfil = PerfilIdosaDoContrato();
            perfil.Deficiencia = true;

            var resultado = _service.AvaliarBeneficio(CatalogoBeneficios.BpcPcd(), perfil);

            Assert.Equal(StatusElegibilidade.Provavel, resultado.Status);
        }

        [Fact]
        public void RN16_BeneficioSemRequisitos_SemprePossivel()
        {
            var resultado = _service.AvaliarBeneficio(CatalogoBeneficios.ValoresAReceber(), new PerfilSocial());

            Assert.Equal(StatusElegibilidade.Possivel, resultado.Status);
        }

        [Theory]
        [InlineData(true, StatusElegibilidade.Provavel)]
        [InlineData(false, StatusElegibilidade.Inelegivel)]
        [InlineData(null, StatusElegibilidade.Possivel)]
        public void RN16_AbonoSalarial_DependeDoVinculoFormal(bool? vinculoFormal, string esperado)
        {
            var perfil = new PerfilSocial { VinculoFormal = vinculoFormal };

            var resultado = _service.AvaliarBeneficio(CatalogoBeneficios.AbonoSalarial(), perfil);

            Assert.Equal(esperado, resultado.Status);
        }

        [Fact]
        public void RN16_TodosOsBeneficiosDoCatalogo_ComOPerfilDoContrato()
        {
            var esperado = new Dictionary<string, string>
            {
                ["TARIFA_SOCIAL"] = StatusElegibilidade.Provavel,
                ["BPC_IDOSO"] = StatusElegibilidade.Provavel,
                ["BPC_PCD"] = StatusElegibilidade.Inelegivel,
                ["VALORES_A_RECEBER"] = StatusElegibilidade.Possivel,
                ["ABONO_SALARIAL"] = StatusElegibilidade.Inelegivel
            };

            foreach (var beneficio in CatalogoBeneficios.Todos())
            {
                var resultado = _service.AvaliarBeneficio(beneficio, PerfilIdosaDoContrato());
                Assert.Equal(esperado[beneficio.Codigo], resultado.Status);
                Assert.False(string.IsNullOrWhiteSpace(resultado.Motivo));
                Assert.True(resultado.Motivo.Length <= 300);
            }
        }

        [Fact]
        public async Task Avaliar_SemPerfilSocial_LancaConsentimentoPendente()
        {
            _perfilRepository.Setup(r => r.ObterAsync(1)).ReturnsAsync((PerfilSocial?)null);

            await Assert.ThrowsAsync<ConsentimentoPendenteException>(() => _service.AvaliarAsync(1));
        }

        [Fact]
        public async Task Avaliar_NaoSobrescreveSolicitadoNemConcedido()
        {
            var concedido = new Elegibilidade { IdElegibilidade = 20, IdUsuario = 1, IdBeneficio = 1, Status = StatusElegibilidade.Concedido, Origem = OrigemElegibilidade.Questionario, DtAvaliacao = _relogio.AgoraUtc.AddDays(-10), DtAtualizacao = _relogio.AgoraUtc.AddDays(-2) };
            var antigo = new Elegibilidade { IdElegibilidade = 21, IdUsuario = 1, IdBeneficio = 2, Status = StatusElegibilidade.Inelegivel, Origem = OrigemElegibilidade.Questionario, DtAvaliacao = _relogio.AgoraUtc.AddDays(-10), DtAtualizacao = _relogio.AgoraUtc.AddDays(-10) };
            var novos = new List<Elegibilidade>();

            _perfilRepository.Setup(r => r.ObterAsync(1)).ReturnsAsync(PerfilIdosaDoContrato());
            _beneficioRepository.Setup(r => r.ListarAtivosAsync()).ReturnsAsync(CatalogoBeneficios.Todos());
            _elegibilidadeRepository.Setup(r => r.ListarDoUsuarioAsync(1)).ReturnsAsync(new List<Elegibilidade> { concedido, antigo });
            _elegibilidadeRepository.Setup(r => r.AdicionarAsync(It.IsAny<Elegibilidade>())).Callback<Elegibilidade>(novos.Add);

            var resultado = await _service.AvaliarAsync(1);

            Assert.Equal(5, resultado.Count);
            Assert.Equal(StatusElegibilidade.Concedido, concedido.Status);
            Assert.Equal(StatusElegibilidade.Provavel, antigo.Status);
            Assert.Equal(3, novos.Count);
            Assert.Equal(StatusElegibilidade.Concedido, resultado[0].Status);
            _unitOfWork.Verify(u => u.SalvarAsync(), Times.Once);
        }

        [Fact]
        public async Task AtualizarStatus_DeConcedidoParaSolicitado_LancaConflito()
        {
            _elegibilidadeRepository.Setup(r => r.ObterPorIdAsync(20)).ReturnsAsync(NovaElegibilidade(1, StatusElegibilidade.Concedido));

            await Assert.ThrowsAsync<ConflitoException>(() =>
                _service.AtualizarStatusAsync(20, new AtualizarElegibilidadeRequest("SOLICITADO"), 1));
        }

        [Fact]
        public async Task AtualizarStatus_DeOutroUsuario_LancaSemPermissao()
        {
            _elegibilidadeRepository.Setup(r => r.ObterPorIdAsync(20)).ReturnsAsync(NovaElegibilidade(2, StatusElegibilidade.Provavel));

            await Assert.ThrowsAsync<SemPermissaoException>(() =>
                _service.AtualizarStatusAsync(20, new AtualizarElegibilidadeRequest("SOLICITADO"), 1));
        }

        [Fact]
        public async Task AtualizarStatus_ProvavelParaSolicitado_Atualiza()
        {
            var elegibilidade = NovaElegibilidade(1, StatusElegibilidade.Provavel);
            _elegibilidadeRepository.Setup(r => r.ObterPorIdAsync(20)).ReturnsAsync(elegibilidade);

            var resposta = await _service.AtualizarStatusAsync(20, new AtualizarElegibilidadeRequest("solicitado"), 1);

            Assert.Equal(StatusElegibilidade.Solicitado, resposta.Status);
            Assert.Equal(_relogio.AgoraUtc, elegibilidade.DtAtualizacao);
        }

        [Fact]
        public async Task AtualizarStatus_Inexistente_LancaNaoEncontrado()
        {
            _elegibilidadeRepository.Setup(r => r.ObterPorIdAsync(99)).ReturnsAsync((Elegibilidade?)null);

            await Assert.ThrowsAsync<NaoEncontradoException>(() =>
                _service.AtualizarStatusAsync(99, new AtualizarElegibilidadeRequest("SOLICITADO"), 1));
        }

        private Elegibilidade NovaElegibilidade(int idUsuario, string status)
        {
            return new Elegibilidade
            {
                IdElegibilidade = 20,
                IdUsuario = idUsuario,
                IdBeneficio = 1,
                Beneficio = CatalogoBeneficios.TarifaSocial(),
                Status = status,
                Origem = OrigemElegibilidade.Questionario,
                DtAvaliacao = _relogio.AgoraUtc.AddDays(-1),
                DtAtualizacao = _relogio.AgoraUtc.AddDays(-1)
            };
        }
    }
}
