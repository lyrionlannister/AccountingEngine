using Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Configurations;

public sealed class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("journal_entry_lines", t =>
            t.HasCheckConstraint(
                "ck_journal_entry_lines_debit_xor_credit",
                "(debit_amount > 0 AND credit_amount = 0) OR (debit_amount = 0 AND credit_amount > 0)"));

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.JournalEntryId).HasColumnName("journal_entry_id").IsRequired();
        builder.Property(e => e.LineNumber).HasColumnName("line_number").IsRequired();
        builder.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
        builder.Property(e => e.CostCenterId).HasColumnName("cost_center_id");
        builder.Property(e => e.PartyId).HasColumnName("party_id");
        builder.Property(e => e.CurrencyId).HasColumnName("currency_id").IsRequired();
        builder.Property(e => e.DebitAmount).HasColumnName("debit_amount").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(e => e.CreditAmount).HasColumnName("credit_amount").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .HasConstraintName("fk_journal_entry_lines_account")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CostCenter>()
            .WithMany()
            .HasForeignKey(e => e.CostCenterId)
            .HasConstraintName("fk_journal_entry_lines_cost_center")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Party>()
            .WithMany()
            .HasForeignKey(e => e.PartyId)
            .HasConstraintName("fk_journal_entry_lines_party")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(e => e.CurrencyId)
            .HasConstraintName("fk_journal_entry_lines_currency")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.JournalEntryId, e.LineNumber }).IsUnique();
        builder.HasIndex(e => e.AccountId);
    }
}
