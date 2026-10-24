using Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Configurations;

public sealed class OpeningBalanceConfiguration : IEntityTypeConfiguration<OpeningBalance>
{
    public void Configure(EntityTypeBuilder<OpeningBalance> builder)
    {
        builder.ToTable("saldos_iniciales", t =>
            t.HasCheckConstraint(
                "ck_saldos_iniciales_debito_xor_credito",
                "(monto_debito > 0 AND monto_credito = 0) OR (monto_debito = 0 AND monto_credito > 0)"));

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.AccountingPeriodId).HasColumnName("periodo_contable_id").IsRequired();
        builder.Property(e => e.AccountId).HasColumnName("cuenta_id").IsRequired();
        builder.Property(e => e.CurrencyId).HasColumnName("moneda_id").IsRequired();
        builder.Property(e => e.DebitAmount).HasColumnName("monto_debito").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(e => e.CreditAmount).HasColumnName("monto_credito").HasColumnType("numeric(18,2)").IsRequired();

        builder.HasOne<AccountingPeriod>()
            .WithMany()
            .HasForeignKey(e => e.AccountingPeriodId)
            .HasConstraintName("fk_saldos_iniciales_periodo_contable")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .HasConstraintName("fk_saldos_iniciales_cuenta")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(e => e.CurrencyId)
            .HasConstraintName("fk_saldos_iniciales_moneda")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.AccountingPeriodId, e.AccountId }).IsUnique();
    }
}
