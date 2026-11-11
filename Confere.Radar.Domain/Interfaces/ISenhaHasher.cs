namespace Confere.Radar.Domain.Interfaces
{
    public interface ISenhaHasher
    {
        string GerarHash(string senha);
        bool Verificar(string senha, string hash);
    }
}
