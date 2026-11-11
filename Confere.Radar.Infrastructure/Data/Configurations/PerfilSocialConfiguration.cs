using Confere.Radar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confere.Radar.Infrastructure.Data.Configurations
{
    public class PerfilSocialConfiguration : IEntityTypeConfiguration<PerfilSocial>
    {
        public void Configure(EntityTypeBuilder<PerfilSocial> builder)
        {
            builder.ToTable("CF_PERFIL_SOCIAL", t => t.HasCheckConstraint("CK_PERFIL_RENDA",
                "[RENDA_PC_FAIXA] IS NULL OR [RENDA_PC_FAIXA] IN ('ATE_1_4','ATE_1_2','ATE_1','ATE_2','ACIMA_2')"));
            builder.HasKey(p => p.IdUsuario);

            builder.Property(p => p.IdUsuario).ValueGeneratedNever();
            builder.Property(p => p.RendaPcFaixa).HasMaxLength(10);

            builder.HasOne(p => p.Usuario)
                .WithOne()
                .HasForeignKey<PerfilSocial>(p => p.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Municipio)
                .WithMany()
                .HasForeignKey(p => p.IdMunicipio)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
