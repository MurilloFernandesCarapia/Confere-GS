using Confere.Radar.Domain.Constants;

namespace Confere.Radar.Domain.Entities
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? SenhaHash { get; set; }
        public string? Telefone { get; set; }
        public int? IdMunicipio { get; set; }
        public string Origem { get; set; } = OrigemUsuario.Radar;
        public bool Ativo { get; set; } = true;
        public DateTime DtCadastro { get; set; }

        public Municipio? Municipio { get; set; }
    }
}
