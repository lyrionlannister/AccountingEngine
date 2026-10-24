using Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Configurations;

public sealed class VoucherTypeConfiguration : IEntityTypeConfiguration<VoucherType>
{
    public void Configure(EntityTypeBuilder<VoucherType> builder)
    {
        builder.ToTable("tipos_comprobante");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.PublicId)
            .HasColumnName("id_publico")
            .HasDefaultValueSql("uuidv7()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(e => e.CompanyId).HasColumnName("empresa_id").IsRequired();
        builder.Property(e => e.Code).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(e => e.NumberingMode).HasColumnName("modo_numeracion").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("activo").IsRequired();

        builder.HasIndex(e => e.PublicId).IsUnique();
        builder.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
    }
}
