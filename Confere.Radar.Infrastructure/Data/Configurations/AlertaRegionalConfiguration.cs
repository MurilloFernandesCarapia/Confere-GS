using Confere.Radar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confere.Radar.Infrastructure.Data.Configurations
{
    public class AlertaRegionalConfiguration : IEntityTypeConfiguration<AlertaRegional>
    {
        public void Configure(EntityTypeBuilder<AlertaRegional> builder)
        {
            builder.ToTable("CF_ALERTA_REGIONAL", t =>
            {
                t.HasCheckConstraint("CK_ALERTA_NIVEL", "[NIVEL] IN ('INFO','ATENCAO','URGENTE')");
                t.HasCheckConstraint("CK_ALERTA_ORIGEM", "[ORIGEM] IN ('AUTOMATICO','GESTOR')");
                t.HasCheckConstraint("CK_ALERTA_PERIODO", "[DT_FIM] > [DT_INICIO]");
            });
            builder.HasKey(a => a.IdAlertaRegional);

            builder.Property(a => a.Titulo).HasMaxLength(120).IsRequired();
            builder.Property(a => a.Mensagem).HasMaxLength(600).IsRequired();
            builder.Property(a => a.Nivel).HasMaxLength(8).IsRequired();
            builder.Property(a => a.Origem).HasMaxLength(10).IsRequired();

            builder.HasIndex(a => new { a.IdMunicipio, a.Ativo, a.DtFim });

            builder.HasOne(a => a.Municipio)
                .WithMany()
                .HasForeignKey(a => a.IdMunicipio)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.TipoGolpe)
                .WithMany()
                .HasForeignKey(a => a.IdTipoGolpe)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Gestor)
                .WithMany()
                .HasForeignKey(a => a.IdGestor)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
