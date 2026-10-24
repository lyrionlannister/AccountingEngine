using Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Configurations;

public sealed class OpeningBalanceConfiguration : IEntityTypeConfiguration<OpeningBalance>
{
    public void Configure(EntityTypeBuilder<OpeningBalance> builder)
    {
        builder.ToTable("opening_balances", t =>
            t.HasCheckConstraint(
                "ck_opening_balances_debit_xor_credit",
                "(debit_amount > 0 AND credit_amount = 0) OR (debit_amount = 0 AND credit_amount > 0)"));

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.AccountingPeriodId).HasColumnName("accounting_period_id").IsRequired();
        builder.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
        builder.Property(e => e.CurrencyId).HasColumnName("currency_id").IsRequired();
        builder.Property(e => e.DebitAmount).HasColumnName("debit_amount").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(e => e.CreditAmount).HasColumnName("credit_amount").HasColumnType("numeric(18,2)").IsRequired();

        builder.HasOne<AccountingPeriod>()
            .WithMany()
            .HasForeignKey(e => e.AccountingPeriodId)
            .HasConstraintName("fk_opening_balances_accounting_period")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .HasConstraintName("fk_opening_balances_account")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(e => e.CurrencyId)
            .HasConstraintName("fk_opening_balances_currency")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.AccountingPeriodId, e.AccountId }).IsUnique();
    }
}
