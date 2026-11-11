using System.Text.RegularExpressions;
using Confere.Radar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Confere.Radar.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Municipio> Municipios => Set<Municipio>();
        public DbSet<TipoGolpe> TiposGolpe => Set<TipoGolpe>();
        public DbSet<OrgaoParceiro> OrgaosParceiros => Set<OrgaoParceiro>();
        public DbSet<Gestor> Gestores => Set<Gestor>();
        public DbSet<IdentificadorSuspeito> Identificadores => Set<IdentificadorSuspeito>();
        public DbSet<Denuncia> Denuncias => Set<Denuncia>();
        public DbSet<AlertaRegional> AlertasRegionais => Set<AlertaRegional>();
        public DbSet<RegistroIdempotencia> RegistrosIdempotencia => Set<RegistroIdempotencia>();
        public DbSet<Beneficio> Beneficios => Set<Beneficio>();
        public DbSet<RequisitoBeneficio> RequisitosBeneficio => Set<RequisitoBeneficio>();
        public DbSet<PassoBeneficio> PassosBeneficio => Set<PassoBeneficio>();
        public DbSet<PerfilSocial> PerfisSociais => Set<PerfilSocial>();
        public DbSet<Elegibilidade> Elegibilidades => Set<Elegibilidade>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            foreach (var entidade in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var propriedade in entidade.GetProperties())
                {
                    propriedade.SetColumnName(ParaMaiusculasComUnderline(propriedade.Name));
                }
            }
        }

        private static string ParaMaiusculasComUnderline(string nome)
        {
            return Regex.Replace(nome, "(?<=[a-z0-9])([A-Z])", "_$1").ToUpperInvariant();
        }
    }
}
