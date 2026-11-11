using Confere.Radar.Domain.Entities;
using Confere.Radar.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confere.Radar.Infrastructure.Data.Configurations
{
    public class TipoGolpeConfiguration : IEntityTypeConfiguration<TipoGolpe>
    {
        public void Configure(EntityTypeBuilder<TipoGolpe> builder)
        {
            builder.ToTable("CF_TIPO_GOLPE", t => t.HasCheckConstraint("CK_TIPO_GOLPE_GRAVIDADE", "[GRAVIDADE] BETWEEN 1 AND 5"));
            builder.HasKey(t => t.IdTipoGolpe);

            builder.Property(t => t.Codigo).HasMaxLength(30).IsRequired();
            builder.Property(t => t.Nome).HasMaxLength(80).IsRequired();
            builder.Property(t => t.Descricao).HasMaxLength(500);
            builder.Property(t => t.OrgaoImitado).HasMaxLength(60);

            builder.HasIndex(t => t.Codigo).IsUnique();

            builder.HasData(DadosIniciais.TiposGolpe);
        }
    }
}
