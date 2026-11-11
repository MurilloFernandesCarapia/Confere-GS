using Confere.Radar.Domain.Models;

namespace Confere.Radar.Domain.Interfaces
{
    public interface INormalizadorIdentificador
    {
        IdentificadorNormalizado Normalizar(string tipo, string valor);
    }
}
