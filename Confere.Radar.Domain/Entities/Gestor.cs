namespace Confere.Radar.Domain.Entities
{
    public class Gestor
    {
        public int IdGestor { get; set; }
        public int IdOrgao { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Cargo { get; set; }
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public bool Ativo { get; set; } = true;
        public DateTime DtCadastro { get; set; }

        public OrgaoParceiro? Orgao { get; set; }
    }
}
