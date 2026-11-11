using Confere.Radar.Domain.Entities;
using Confere.Radar.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confere.Radar.Infrastructure.Data.Configurations
{
    public class BeneficioConfiguration : IEntityTypeConfiguration<Beneficio>
    {
        public void Configure(EntityTypeBuilder<Beneficio> builder)
        {
            builder.ToTable("CF_BENEFICIO");
            builder.HasKey(b => b.IdBeneficio);

            builder.Property(b => b.Codigo).HasMaxLength(30).IsRequired();
            builder.Property(b => b.Nome).HasMaxLength(100).IsRequired();
            builder.Property(b => b.OrgaoResponsavel).HasMaxLength(80).IsRequired();
            builder.Property(b => b.DescricaoSimples).HasMaxLength(600).IsRequired();
            builder.Property(b => b.LinkOficial).HasMaxLength(300).IsRequired();
            builder.Property(b => b.ValorReferencia).HasMaxLength(80);

            builder.HasIndex(b => b.Codigo).IsUnique();

            builder.HasData(DadosIniciais.Beneficios);
        }
    }
}
