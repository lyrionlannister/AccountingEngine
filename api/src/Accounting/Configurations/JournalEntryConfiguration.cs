using Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Configurations;

public sealed class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("journal_entries");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.PublicId)
            .HasColumnName("public_id")
            .HasDefaultValueSql("uuidv7()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(e => e.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(e => e.VoucherTypeId).HasColumnName("voucher_type_id").IsRequired();
        builder.Property(e => e.AccountingPeriodId).HasColumnName("accounting_period_id").IsRequired();
        builder.Property(e => e.SequenceNumber).HasColumnName("sequence_number");
        builder.Property(e => e.Status).HasColumnName("journal_entry_status").HasColumnType("journal_entry_status").IsRequired();
        builder.Property(e => e.EntryDate).HasColumnName("entry_date").IsRequired();
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
        builder.Property(e => e.SourceModule).HasColumnName("source_module").HasMaxLength(100);
        builder.Property(e => e.SourceReference).HasColumnName("source_reference").HasMaxLength(100);
        builder.Property(e => e.ReversalOfEntryId).HasColumnName("reversal_of_entry_id");
        builder.Property(e => e.PostedAt).HasColumnName("posted_at");
        builder.Property(e => e.PostedBy).HasColumnName("posted_by").HasMaxLength(100);

        builder.HasOne<VoucherType>()
            .WithMany()
            .HasForeignKey(e => e.VoucherTypeId)
            .HasConstraintName("fk_journal_entries_voucher_type")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AccountingPeriod>()
            .WithMany()
            .HasForeignKey(e => e.AccountingPeriodId)
            .HasConstraintName("fk_journal_entries_accounting_period")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<JournalEntry>()
            .WithMany()
            .HasForeignKey(e => e.ReversalOfEntryId)
            .HasConstraintName("fk_journal_entries_reversal_of_entry")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Lines)
            .WithOne()
            .HasForeignKey(l => l.JournalEntryId)
            .HasConstraintName("fk_journal_entry_lines_journal_entry")
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(e => e.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(e => e.PublicId).IsUnique();
        builder.HasIndex(e => new { e.CompanyId, e.VoucherTypeId, e.SequenceNumber })
            .IsUnique()
            .HasDatabaseName("ix_journal_entries_company_voucher_sequence");
        builder.HasIndex(e => new { e.CompanyId, e.AccountingPeriodId, e.Status })
            .HasDatabaseName("ix_journal_entries_company_period_status");
        builder.HasIndex(e => new { e.CompanyId, e.SourceModule, e.SourceReference })
            .IsUnique()
            .HasDatabaseName("ix_journal_entries_idempotency_source")
            .HasFilter("source_reference IS NOT NULL");
    }
}
