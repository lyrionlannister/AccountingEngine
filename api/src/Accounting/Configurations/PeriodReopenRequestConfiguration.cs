using Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Configurations;

public sealed class PeriodReopenRequestConfiguration : IEntityTypeConfiguration<PeriodReopenRequest>
{
    public void Configure(EntityTypeBuilder<PeriodReopenRequest> builder)
    {
        builder.ToTable("solicitudes_reapertura_periodo");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.PublicId)
            .HasColumnName("id_publico")
            .HasDefaultValueSql("uuidv7()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(e => e.CompanyId).HasColumnName("empresa_id").IsRequired();
        builder.Property(e => e.AccountingPeriodId).HasColumnName("periodo_contable_id").IsRequired();
        builder.Property(e => e.Reason).HasColumnName("motivo").HasMaxLength(1000).IsRequired();
        builder.Property(e => e.RequestedBy).HasColumnName("solicitado_por").HasMaxLength(100).IsRequired();
        builder.Property(e => e.RequestedAt).HasColumnName("solicitado_en").IsRequired();
        builder.Property(e => e.Status).HasColumnName("estado").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.ResolvedBy).HasColumnName("resuelto_por").HasMaxLength(100);
        builder.Property(e => e.ResolvedAt).HasColumnName("resuelto_en");
        builder.Property(e => e.ResolutionNotes).HasColumnName("notas_resolucion").HasMaxLength(1000);

        builder.HasOne<AccountingPeriod>()
            .WithMany()
            .HasForeignKey(e => e.AccountingPeriodId)
            .HasConstraintName("fk_solicitudes_reapertura_periodo_periodo_contable")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.PublicId).IsUnique();
        builder.HasIndex(e => new { e.CompanyId, e.AccountingPeriodId, e.Status })
            .HasDatabaseName("ix_solicitudes_reapertura_periodo_empresa_periodo_estado");
    }
}
