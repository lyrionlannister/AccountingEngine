using Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Configurations;

public sealed class AccountingPeriodConfiguration : IEntityTypeConfiguration<AccountingPeriod>
{
    public void Configure(EntityTypeBuilder<AccountingPeriod> builder)
    {
        builder.ToTable("periodos_contables");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.PublicId)
            .HasColumnName("id_publico")
            .HasDefaultValueSql("uuidv7()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(e => e.CompanyId).HasColumnName("empresa_id").IsRequired();
        builder.Property(e => e.Name).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(e => e.StartDate).HasColumnName("fecha_inicio").IsRequired();
        builder.Property(e => e.EndDate).HasColumnName("fecha_fin").IsRequired();
        builder.Property(e => e.Status).HasColumnName("estado").HasColumnType("estado_periodo").IsRequired();
        builder.Property(e => e.ClosedAt).HasColumnName("cerrado_en");
        builder.Property(e => e.ClosedBy).HasColumnName("cerrado_por").HasMaxLength(100);

        builder.HasIndex(e => e.PublicId).IsUnique();
        builder.HasIndex(e => new { e.CompanyId, e.Name }).IsUnique();
        builder.HasIndex(e => new { e.CompanyId, e.StartDate, e.EndDate });
    }
}
