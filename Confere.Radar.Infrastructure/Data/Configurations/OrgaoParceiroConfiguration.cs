using Confere.Radar.Domain.Entities;
using Confere.Radar.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confere.Radar.Infrastructure.Data.Configurations
{
    public class OrgaoParceiroConfiguration : IEntityTypeConfiguration<OrgaoParceiro>
    {
        public void Configure(EntityTypeBuilder<OrgaoParceiro> builder)
        {
            builder.ToTable("CF_ORGAO_PARCEIRO", t => t.HasCheckConstraint("CK_ORGAO_TIPO",
                "[TIPO] IN ('CRAS','PROCON','PREFEITURA','BANCO','COOPERATIVA','ONG')"));
            builder.HasKey(o => o.IdOrgao);

            builder.Property(o => o.Nome).HasMaxLength(120).IsRequired();
            builder.Property(o => o.Tipo).HasMaxLength(15).IsRequired();
            builder.Property(o => o.EmailContato).HasMaxLength(150);

            builder.HasOne(o => o.Municipio)
                .WithMany()
                .HasForeignKey(o => o.IdMunicipio)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(DadosIniciais.OrgaosParceiros);
        }
    }
}
