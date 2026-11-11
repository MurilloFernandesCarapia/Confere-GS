using Confere.Radar.Domain.Entities;
using Confere.Radar.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confere.Radar.Infrastructure.Data.Configurations
{
    public class MunicipioConfiguration : IEntityTypeConfiguration<Municipio>
    {
        public void Configure(EntityTypeBuilder<Municipio> builder)
        {
            builder.ToTable("CF_MUNICIPIO");
            builder.HasKey(m => m.IdMunicipio);

            builder.Property(m => m.Nome).HasMaxLength(120).IsRequired();
            builder.Property(m => m.Uf).HasMaxLength(2).IsFixedLength().IsRequired();

            builder.HasIndex(m => m.CodIbge).IsUnique();

            builder.HasData(DadosIniciais.Municipios);
        }
    }
}
