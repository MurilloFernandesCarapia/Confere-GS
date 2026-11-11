using Confere.Radar.Domain.Interfaces;

namespace Confere.Radar.Infrastructure.Security
{
    public class BCryptSenhaHasher : ISenhaHasher
    {
        private const int FatorDeCusto = 11;

        public string GerarHash(string senha)
        {
            return BCrypt.Net.BCrypt.HashPassword(senha, FatorDeCusto);
        }

        public bool Verificar(string senha, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(senha, hash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
        }
    }
}
