using Confere.Radar.Application.Services;
using Confere.Radar.Domain.Entities;
using Confere.Radar.Domain.Exceptions;
using Confere.Radar.Domain.Interfaces;
using Confere.Radar.UnitTests.Helpers;
using Moq;

namespace Confere.Radar.UnitTests.Services
{
    public class BeneficioServiceTests
    {
        private readonly Mock<IBeneficioRepository> _repository = new();
        private readonly BeneficioService _service;

        public BeneficioServiceTests()
        {
            _service = new BeneficioService(_repository.Object);
        }

        [Fact]
        public async Task Listar_DevolveOCatalogo()
        {
            _repository.Setup(r => r.ListarAtivosAsync()).ReturnsAsync(CatalogoBeneficios.Todos());

            var beneficios = await _service.ListarAsync();

            Assert.Equal(5, beneficios.Count);
        }

        [Fact]
        public async Task ObterPorCodigo_NormalizaOCodigoEOrdenaPassos()
        {
            var tarifa = CatalogoBeneficios.TarifaSocial();
            tarifa.Passos.Add(new PassoBeneficio { Ordem = 2, Instrucao = "Ligue para a distribuidora", Canal = "TELEFONE" });
            tarifa.Passos.Add(new PassoBeneficio { Ordem = 1, Instrucao = "Faça o CadÚnico no CRAS", Canal = "PRESENCIAL" });
            _repository.Setup(r => r.ObterPorCodigoAsync("TARIFA_SOCIAL")).ReturnsAsync(tarifa);

            var detalhe = await _service.ObterPorCodigoAsync(" tarifa_social ");

            Assert.Equal(new[] { 1, 2 }, detalhe.Passos.Select(p => p.Ordem));
            Assert.Equal(3, detalhe.Requisitos.Count);
        }

        [Fact]
        public async Task ObterPorCodigo_Inexistente_LancaNaoEncontrado()
        {
            _repository.Setup(r => r.ObterPorCodigoAsync("XYZ")).ReturnsAsync((Beneficio?)null);

            await Assert.ThrowsAsync<NaoEncontradoException>(() => _service.ObterPorCodigoAsync("xyz"));
        }
    }
}
