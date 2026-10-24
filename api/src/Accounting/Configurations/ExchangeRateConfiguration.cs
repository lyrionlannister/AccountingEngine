using Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Configurations;

public sealed class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("tipos_cambio");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.FromCurrencyId).HasColumnName("moneda_origen_id").IsRequired();
        builder.Property(e => e.ToCurrencyId).HasColumnName("moneda_destino_id").IsRequired();
        builder.Property(e => e.Rate).HasColumnName("tasa").HasColumnType("numeric(18,8)").IsRequired();
        builder.Property(e => e.EffectiveDate).HasColumnName("fecha_vigencia").IsRequired();
        builder.Property(e => e.Source).HasColumnName("fuente").HasMaxLength(100).IsRequired();

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(e => e.FromCurrencyId)
            .HasConstraintName("fk_tipos_cambio_moneda_origen")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(e => e.ToCurrencyId)
            .HasConstraintName("fk_tipos_cambio_moneda_destino")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.FromCurrencyId, e.ToCurrencyId, e.EffectiveDate })
            .IsUnique()
            .HasDatabaseName("ix_tipos_cambio_par_fecha");
    }
}
