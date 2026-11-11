using Confere.Radar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confere.Radar.Infrastructure.Data.Configurations
{
    public class ElegibilidadeConfiguration : IEntityTypeConfiguration<Elegibilidade>
    {
        public void Configure(EntityTypeBuilder<Elegibilidade> builder)
        {
            builder.ToTable("CF_ELEGIBILIDADE", t =>
            {
                t.HasCheckConstraint("CK_ELEGIBILIDADE_STATUS",
                    "[STATUS] IN ('PROVAVEL','POSSIVEL','INELEGIVEL','SOLICITADO','CONCEDIDO')");
                t.HasCheckConstraint("CK_ELEGIBILIDADE_ORIGEM", "[ORIGEM] IN ('QUESTIONARIO','DOCUMENTO_IA')");
            });
            builder.HasKey(e => e.IdElegibilidade);

            builder.Property(e => e.Status).HasMaxLength(12).IsRequired();
            builder.Property(e => e.Motivo).HasMaxLength(300);
            builder.Property(e => e.Origem).HasMaxLength(15).IsRequired();

            builder.HasIndex(e => new { e.IdUsuario, e.IdBeneficio }).IsUnique();

            builder.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Beneficio)
                .WithMany()
                .HasForeignKey(e => e.IdBeneficio)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
