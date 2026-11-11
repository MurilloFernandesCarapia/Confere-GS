using Confere.Radar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confere.Radar.Infrastructure.Data.Configurations
{
    public class IdentificadorSuspeitoConfiguration : IEntityTypeConfiguration<IdentificadorSuspeito>
    {
        public void Configure(EntityTypeBuilder<IdentificadorSuspeito> builder)
        {
            builder.ToTable("CF_IDENTIFICADOR_SUSPEITO", t =>
            {
                t.HasCheckConstraint("CK_IDENTIFICADOR_TIPO", "[TIPO] IN ('TELEFONE','LINK','CHAVE_PIX','EMAIL','PERFIL')");
                t.HasCheckConstraint("CK_IDENTIFICADOR_STATUS", "[STATUS] IN ('EM_ANALISE','CONFIRMADO','DESCARTADO')");
                t.HasCheckConstraint("CK_IDENTIFICADOR_SCORE", "[SCORE] BETWEEN 0 AND 100");
            });
            builder.HasKey(i => i.IdIdentificador);

            builder.Property(i => i.Tipo).HasMaxLength(10).IsRequired();
            builder.Property(i => i.ValorHash).HasMaxLength(64).IsRequired();
            builder.Property(i => i.ValorMascarado).HasMaxLength(120).IsRequired();
            builder.Property(i => i.Status).HasMaxLength(12).IsRequired();

            builder.HasIndex(i => new { i.Tipo, i.ValorHash }).IsUnique();
        }
    }
}
