using Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Configurations;

public sealed class AccountBalanceSnapshotConfiguration : IEntityTypeConfiguration<AccountBalanceSnapshot>
{
    public void Configure(EntityTypeBuilder<AccountBalanceSnapshot> builder)
    {
        builder.ToTable("saldos_cuenta");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.AccountingPeriodId).HasColumnName("periodo_contable_id").IsRequired();
        builder.Property(e => e.AccountId).HasColumnName("cuenta_id").IsRequired();
        builder.Property(e => e.DebitTotal).HasColumnName("total_debito").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(e => e.CreditTotal).HasColumnName("total_credito").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(e => e.CalculatedAt).HasColumnName("calculado_en").IsRequired();

        builder.HasOne<AccountingPeriod>()
            .WithMany()
            .HasForeignKey(e => e.AccountingPeriodId)
            .HasConstraintName("fk_saldos_cuenta_periodo_contable")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .HasConstraintName("fk_saldos_cuenta_cuenta")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.AccountingPeriodId, e.AccountId }).IsUnique();
    }
}
