using Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Configurations;

public sealed class AccountBalanceSnapshotConfiguration : IEntityTypeConfiguration<AccountBalanceSnapshot>
{
    public void Configure(EntityTypeBuilder<AccountBalanceSnapshot> builder)
    {
        builder.ToTable("account_balance_snapshots");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.AccountingPeriodId).HasColumnName("accounting_period_id").IsRequired();
        builder.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
        builder.Property(e => e.DebitTotal).HasColumnName("debit_total").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(e => e.CreditTotal).HasColumnName("credit_total").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(e => e.CalculatedAt).HasColumnName("calculated_at").IsRequired();

        builder.HasOne<AccountingPeriod>()
            .WithMany()
            .HasForeignKey(e => e.AccountingPeriodId)
            .HasConstraintName("fk_account_balance_snapshots_accounting_period")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .HasConstraintName("fk_account_balance_snapshots_account")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.AccountingPeriodId, e.AccountId }).IsUnique();
    }
}
