using Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Configurations;

public sealed class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("asientos_contables");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.PublicId)
            .HasColumnName("id_publico")
            .HasDefaultValueSql("uuidv7()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(e => e.CompanyId).HasColumnName("empresa_id").IsRequired();
        builder.Property(e => e.VoucherTypeId).HasColumnName("tipo_comprobante_id").IsRequired();
        builder.Property(e => e.AccountingPeriodId).HasColumnName("periodo_contable_id").IsRequired();
        builder.Property(e => e.SequenceNumber).HasColumnName("numero_secuencia");
        builder.Property(e => e.Status).HasColumnName("estado").HasColumnType("estado_asiento").IsRequired();
        builder.Property(e => e.EntryDate).HasColumnName("fecha_asiento").IsRequired();
        builder.Property(e => e.Description).HasColumnName("descripcion").HasMaxLength(500).IsRequired();
        builder.Property(e => e.SourceModule).HasColumnName("modulo_origen").HasMaxLength(100);
        builder.Property(e => e.SourceReference).HasColumnName("referencia_origen").HasMaxLength(100);
        builder.Property(e => e.ReversalOfEntryId).HasColumnName("asiento_revertido_id");
        builder.Property(e => e.PostedAt).HasColumnName("contabilizado_en");
        builder.Property(e => e.PostedBy).HasColumnName("contabilizado_por").HasMaxLength(100);

        builder.HasOne<VoucherType>()
            .WithMany()
            .HasForeignKey(e => e.VoucherTypeId)
            .HasConstraintName("fk_asientos_contables_tipo_comprobante")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AccountingPeriod>()
            .WithMany()
            .HasForeignKey(e => e.AccountingPeriodId)
            .HasConstraintName("fk_asientos_contables_periodo_contable")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<JournalEntry>()
            .WithMany()
            .HasForeignKey(e => e.ReversalOfEntryId)
            .HasConstraintName("fk_asientos_contables_asiento_revertido")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Lines)
            .WithOne()
            .HasForeignKey(l => l.JournalEntryId)
            .HasConstraintName("fk_lineas_asiento_asiento_contable")
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(e => e.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(e => e.PublicId).IsUnique();
        builder.HasIndex(e => new { e.CompanyId, e.VoucherTypeId, e.SequenceNumber })
            .IsUnique()
            .HasDatabaseName("ix_asientos_contables_empresa_comprobante_secuencia");
        builder.HasIndex(e => new { e.CompanyId, e.AccountingPeriodId, e.Status })
            .HasDatabaseName("ix_asientos_contables_empresa_periodo_estado");
        builder.HasIndex(e => new { e.CompanyId, e.SourceModule, e.SourceReference })
            .IsUnique()
            .HasDatabaseName("ix_asientos_contables_idempotencia_origen")
            .HasFilter("referencia_origen IS NOT NULL AND asiento_revertido_id IS NULL");

        builder.HasIndex(e => e.ReversalOfEntryId)
            .IsUnique()
            .HasDatabaseName("ix_asientos_contables_reversion_unica")
            .HasFilter("asiento_revertido_id IS NOT NULL");
    }
}
