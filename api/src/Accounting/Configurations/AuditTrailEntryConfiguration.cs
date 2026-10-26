using Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Configurations;

public sealed class AuditTrailEntryConfiguration : IEntityTypeConfiguration<AuditTrailEntry>
{
    public void Configure(EntityTypeBuilder<AuditTrailEntry> builder)
    {
        builder.ToTable("auditoria_contable", t =>
            t.HasCheckConstraint(
                "ck_auditoria_contable_una_referencia",
                "(CASE WHEN asiento_id IS NOT NULL THEN 1 ELSE 0 END) + "
                + "(CASE WHEN periodo_contable_id IS NOT NULL THEN 1 ELSE 0 END) + "
                + "(CASE WHEN solicitud_reapertura_id IS NOT NULL THEN 1 ELSE 0 END) + "
                + "(CASE WHEN saldo_cuenta_id IS NOT NULL THEN 1 ELSE 0 END) = 1"));

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.PublicId)
            .HasColumnName("id_publico")
            .HasDefaultValueSql("uuidv7()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(e => e.CompanyId).HasColumnName("empresa_id").IsRequired();
        builder.Property(e => e.Operation).HasColumnName("operacion").HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(e => e.Actor).HasColumnName("actor").HasMaxLength(100).IsRequired();
        builder.Property(e => e.OccurredAt).HasColumnName("ocurrido_en").IsRequired();
        builder.Property(e => e.JournalEntryId).HasColumnName("asiento_id");
        builder.Property(e => e.AccountingPeriodId).HasColumnName("periodo_contable_id");
        builder.Property(e => e.PeriodReopenRequestId).HasColumnName("solicitud_reapertura_id");
        builder.Property(e => e.AccountBalanceSnapshotId).HasColumnName("saldo_cuenta_id");
        builder.Property(e => e.Notes).HasColumnName("notas").HasMaxLength(1000);

        builder.HasOne(e => e.JournalEntry)
            .WithMany()
            .HasForeignKey(e => e.JournalEntryId)
            .HasConstraintName("fk_auditoria_contable_asiento")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AccountingPeriod)
            .WithMany()
            .HasForeignKey(e => e.AccountingPeriodId)
            .HasConstraintName("fk_auditoria_contable_periodo_contable")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PeriodReopenRequest)
            .WithMany()
            .HasForeignKey(e => e.PeriodReopenRequestId)
            .HasConstraintName("fk_auditoria_contable_solicitud_reapertura")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AccountBalanceSnapshot)
            .WithMany()
            .HasForeignKey(e => e.AccountBalanceSnapshotId)
            .HasConstraintName("fk_auditoria_contable_saldo_cuenta")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.PublicId).IsUnique();
        builder.HasIndex(e => new { e.CompanyId, e.OccurredAt }).HasDatabaseName("ix_auditoria_contable_empresa_fecha");
    }
}
