using Confere.Radar.Domain.Entities;
using Confere.Radar.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confere.Radar.Infrastructure.Data.Configurations
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("CF_USUARIO", t => t.HasCheckConstraint("CK_USUARIO_ORIGEM", "[ORIGEM] IN ('RADAR','JAVA')"));
            builder.HasKey(u => u.IdUsuario);

            builder.Property(u => u.Nome).HasMaxLength(120).IsRequired();
            builder.Property(u => u.Email).HasMaxLength(150).IsRequired();
            builder.Property(u => u.SenhaHash).HasMaxLength(100);
            builder.Property(u => u.Telefone).HasMaxLength(20);
            builder.Property(u => u.Origem).HasMaxLength(10).IsRequired();

            builder.HasIndex(u => u.Email).IsUnique();

            builder.HasOne(u => u.Municipio)
                .WithMany()
                .HasForeignKey(u => u.IdMunicipio)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(DadosIniciais.Usuarios);
        }
    }
}
