using Confere.Radar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confere.Radar.Infrastructure.Data.Configurations
{
    public class DenunciaConfiguration : IEntityTypeConfiguration<Denuncia>
    {
        public void Configure(EntityTypeBuilder<Denuncia> builder)
        {
            builder.ToTable("CF_DENUNCIA");
            builder.HasKey(d => d.IdDenuncia);

            builder.Property(d => d.Descricao).HasMaxLength(500);

            builder.HasIndex(d => new { d.IdMunicipio, d.DtDenuncia });
            builder.HasIndex(d => new { d.IdUsuario, d.IdIdentificador, d.DtDenuncia });

            builder.HasOne(d => d.Usuario)
                .WithMany()
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Identificador)
                .WithMany(i => i.Denuncias)
                .HasForeignKey(d => d.IdIdentificador)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.TipoGolpe)
                .WithMany()
                .HasForeignKey(d => d.IdTipoGolpe)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Municipio)
                .WithMany()
                .HasForeignKey(d => d.IdMunicipio)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
