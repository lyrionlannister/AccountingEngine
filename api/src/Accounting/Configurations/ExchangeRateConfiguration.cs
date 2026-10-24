using Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Configurations;

public sealed class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("exchange_rates");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.FromCurrencyId).HasColumnName("from_currency_id").IsRequired();
        builder.Property(e => e.ToCurrencyId).HasColumnName("to_currency_id").IsRequired();
        builder.Property(e => e.Rate).HasColumnName("rate").HasColumnType("numeric(18,8)").IsRequired();
        builder.Property(e => e.EffectiveDate).HasColumnName("effective_date").IsRequired();
        builder.Property(e => e.Source).HasColumnName("source").HasMaxLength(100).IsRequired();

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(e => e.FromCurrencyId)
            .HasConstraintName("fk_exchange_rates_from_currency")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(e => e.ToCurrencyId)
            .HasConstraintName("fk_exchange_rates_to_currency")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.FromCurrencyId, e.ToCurrencyId, e.EffectiveDate })
            .IsUnique()
            .HasDatabaseName("ix_exchange_rates_pair_date");
    }
}
