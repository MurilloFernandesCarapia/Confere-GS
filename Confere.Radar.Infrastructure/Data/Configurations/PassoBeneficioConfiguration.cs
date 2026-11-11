using Confere.Radar.Domain.Entities;
using Confere.Radar.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confere.Radar.Infrastructure.Data.Configurations
{
    public class PassoBeneficioConfiguration : IEntityTypeConfiguration<PassoBeneficio>
    {
        public void Configure(EntityTypeBuilder<PassoBeneficio> builder)
        {
            builder.ToTable("CF_PASSO_BENEFICIO", t => t.HasCheckConstraint("CK_PASSO_CANAL",
                "[CANAL] IN ('APP','SITE','TELEFONE','PRESENCIAL')"));
            builder.HasKey(p => p.IdPasso);

            builder.Property(p => p.Instrucao).HasMaxLength(400).IsRequired();
            builder.Property(p => p.Canal).HasMaxLength(12).IsRequired();

            builder.HasIndex(p => new { p.IdBeneficio, p.Ordem }).IsUnique();

            builder.HasOne(p => p.Beneficio)
                .WithMany(b => b.Passos)
                .HasForeignKey(p => p.IdBeneficio)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasData(DadosIniciais.Passos);
        }
    }
}
