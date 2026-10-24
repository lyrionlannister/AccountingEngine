using Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Configurations;

public sealed class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("monedas");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.Code).HasColumnName("codigo").HasMaxLength(3).IsRequired();
        builder.Property(e => e.Name).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(e => e.DecimalPlaces).HasColumnName("decimales").IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("activo").IsRequired();

        builder.HasIndex(e => e.Code).IsUnique();
    }
}
