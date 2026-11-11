using Confere.Radar.Domain.Entities;
using Confere.Radar.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confere.Radar.Infrastructure.Data.Configurations
{
    public class GestorConfiguration : IEntityTypeConfiguration<Gestor>
    {
        public void Configure(EntityTypeBuilder<Gestor> builder)
        {
            builder.ToTable("CF_GESTOR");
            builder.HasKey(g => g.IdGestor);

            builder.Property(g => g.Nome).HasMaxLength(120).IsRequired();
            builder.Property(g => g.Cargo).HasMaxLength(60);
            builder.Property(g => g.Email).HasMaxLength(150).IsRequired();
            builder.Property(g => g.SenhaHash).HasMaxLength(100).IsRequired();

            builder.HasIndex(g => g.Email).IsUnique();

            builder.HasOne(g => g.Orgao)
                .WithMany(o => o.Gestores)
                .HasForeignKey(g => g.IdOrgao)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(DadosIniciais.Gestores);
        }
    }
}
