using Confere.Radar.Domain.Entities;
using Confere.Radar.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confere.Radar.Infrastructure.Data.Configurations
{
    public class RequisitoBeneficioConfiguration : IEntityTypeConfiguration<RequisitoBeneficio>
    {
        public void Configure(EntityTypeBuilder<RequisitoBeneficio> builder)
        {
            builder.ToTable("CF_REQUISITO_BENEFICIO", t =>
            {
                t.HasCheckConstraint("CK_REQUISITO_CRITERIO",
                    "[CRITERIO] IN ('IDADE_MIN','RENDA_PC_MAX','CADUNICO','BPC','DEFICIENCIA','VINCULO_FORMAL')");
                t.HasCheckConstraint("CK_REQUISITO_OPERADOR", "[OPERADOR] IN ('>=','<=','=')");
            });
            builder.HasKey(r => r.IdRequisito);

            builder.Property(r => r.Criterio).HasMaxLength(20).IsRequired();
            builder.Property(r => r.Operador).HasMaxLength(2).IsRequired();
            builder.Property(r => r.Valor).HasMaxLength(20).IsRequired();

            builder.HasOne(r => r.Beneficio)
                .WithMany(b => b.Requisitos)
                .HasForeignKey(r => r.IdBeneficio)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasData(DadosIniciais.Requisitos);
        }
    }
}
