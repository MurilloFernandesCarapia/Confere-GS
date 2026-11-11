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
    public class PerfilSocialServiceTests
    {
        private readonly Mock<IPerfilSocialRepository> _perfilRepository = new();
        private readonly Mock<IElegibilidadeRepository> _elegibilidadeRepository = new();
        private readonly Mock<IMunicipioRepository> _municipioRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly RelogioFixo _relogio = new();
        private readonly PerfilSocialService _service;

        public PerfilSocialServiceTests()
        {
            _municipioRepository.Setup(r => r.ObterPorCodIbgeAsync(3550308))
                .ReturnsAsync(new Municipio { IdMunicipio = 1, Nome = "São Paulo", Uf = "SP", CodIbge = 3550308 });

            _service = new PerfilSocialService(
                _perfilRepository.Object,
                _elegibilidadeRepository.Object,
                _municipioRepository.Object,
                _unitOfWork.Object,
                _relogio,
                NullLogger<PerfilSocialService>.Instance);
        }

        [Fact]
        public async Task RN17_SemConsentimento_NaoGrava()
        {
            var request = NovoRequest() with { Consentimento = false };

            await Assert.ThrowsAsync<ConsentimentoPendenteException>(() => _service.SalvarAsync(request, 1));

            _perfilRepository.Verify(r => r.AdicionarAsync(It.IsAny<PerfilSocial>()), Times.Never);
            _unitOfWork.Verify(u => u.SalvarAsync(), Times.Never);
        }

        [Fact]
        public async Task RN17_ComConsentimento_CriaPerfilComDataDoConsentimento()
        {
            PerfilSocial? criado = null;
            _perfilRepository.Setup(r => r.ObterAsync(1)).ReturnsAsync((PerfilSocial?)null);
            _perfilRepository.Setup(r => r.AdicionarAsync(It.IsAny<PerfilSocial>())).Callback<PerfilSocial>(p => criado = p);

            var resposta = await _service.SalvarAsync(NovoRequest(), 1);

            Assert.NotNull(criado);
            Assert.Equal(_relogio.AgoraUtc, criado!.DtConsentimento);
            Assert.Equal(FaixaRenda.AteUmQuarto, criado.RendaPcFaixa);
            Assert.Equal("até 1/4 do salário mínimo", resposta.RendaPcFaixaDescricao);
            Assert.Equal(3550308, resposta.Municipio!.CodIbge);
        }

        [Fact]
        public async Task Salvar_PerfilExistente_AtualizaSemCriarOutro()
        {
            var existente = new PerfilSocial { IdUsuario = 1, Idade = 30 };
            _perfilRepository.Setup(r => r.ObterAsync(1)).ReturnsAsync(existente);

            await _service.SalvarAsync(NovoRequest(), 1);

            Assert.Equal(68, existente.Idade);
            _perfilRepository.Verify(r => r.AdicionarAsync(It.IsAny<PerfilSocial>()), Times.Never);
        }

        [Fact]
        public async Task Salvar_MunicipioInexistente_LancaValidacao()
        {
            _municipioRepository.Setup(r => r.ObterPorCodIbgeAsync(1234567)).ReturnsAsync((Municipio?)null);

            await Assert.ThrowsAsync<ValidacaoException>(() => _service.SalvarAsync(NovoRequest() with { CodIbgeMunicipio = 1234567 }, 1));
        }

        [Fact]
        public async Task Obter_SemPerfil_LancaNaoEncontrado()
        {
            _perfilRepository.Setup(r => r.ObterAsync(1)).ReturnsAsync((PerfilSocial?)null);

            await Assert.ThrowsAsync<NaoEncontradoException>(() => _service.ObterAsync(1));
        }

        [Fact]
        public async Task Excluir_ApagaResultadosCalculadosEMantemOsMarcadosPeloCidadao()
        {
            var perfil = new PerfilSocial { IdUsuario = 1 };
            var provavel = new Elegibilidade { IdElegibilidade = 1, Status = StatusElegibilidade.Provavel };
            var solicitado = new Elegibilidade { IdElegibilidade = 2, Status = StatusElegibilidade.Solicitado };
            var concedido = new Elegibilidade { IdElegibilidade = 3, Status = StatusElegibilidade.Concedido };

            _perfilRepository.Setup(r => r.ObterAsync(1)).ReturnsAsync(perfil);
            _elegibilidadeRepository.Setup(r => r.ListarDoUsuarioAsync(1)).ReturnsAsync(new List<Elegibilidade> { provavel, solicitado, concedido });

            await _service.ExcluirAsync(1);

            _perfilRepository.Verify(r => r.Remover(perfil), Times.Once);
            _elegibilidadeRepository.Verify(r => r.Remover(provavel), Times.Once);
            _elegibilidadeRepository.Verify(r => r.Remover(solicitado), Times.Never);
            _elegibilidadeRepository.Verify(r => r.Remover(concedido), Times.Never);
        }

        private static PerfilSocialRequest NovoRequest()
        {
            return new PerfilSocialRequest(68, "ate_1_4", 2, true, false, false, false, 3550308, true);
        }
    }
}
