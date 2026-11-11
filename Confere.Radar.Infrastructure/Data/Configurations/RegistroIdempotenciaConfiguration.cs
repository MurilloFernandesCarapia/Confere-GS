using Confere.Radar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confere.Radar.Infrastructure.Data.Configurations
{
    public class RegistroIdempotenciaConfiguration : IEntityTypeConfiguration<RegistroIdempotencia>
    {
        public void Configure(EntityTypeBuilder<RegistroIdempotencia> builder)
        {
            builder.ToTable("CF_IDEMPOTENCIA", t => t.HasCheckConstraint("CK_IDEMPOTENCIA_SITUACAO",
                "[SITUACAO] IN ('PROCESSANDO','CONCLUIDA')"));
            builder.HasKey(r => r.Chave);

            builder.Property(r => r.Chave).HasMaxLength(64);
            builder.Property(r => r.Endpoint).HasMaxLength(120).IsRequired();
            builder.Property(r => r.HashRequisicao).HasMaxLength(64).IsRequired();
            builder.Property(r => r.Situacao).HasMaxLength(12).IsRequired();

            builder.HasIndex(r => r.DtExpiracao);
        }
    }
}
