using Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Configurations;

public sealed class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("lineas_asiento", t =>
            t.HasCheckConstraint(
                "ck_lineas_asiento_debito_xor_credito",
                "(monto_debito > 0 AND monto_credito = 0) OR (monto_debito = 0 AND monto_credito > 0)"));

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.JournalEntryId).HasColumnName("asiento_contable_id").IsRequired();
        builder.Property(e => e.LineNumber).HasColumnName("numero_linea").IsRequired();
        builder.Property(e => e.AccountId).HasColumnName("cuenta_id").IsRequired();
        builder.Property(e => e.CostCenterId).HasColumnName("centro_costo_id");
        builder.Property(e => e.PartyId).HasColumnName("tercero_id");
        builder.Property(e => e.CurrencyId).HasColumnName("moneda_id").IsRequired();
        builder.Property(e => e.DebitAmount).HasColumnName("monto_debito").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(e => e.CreditAmount).HasColumnName("monto_credito").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(e => e.Description).HasColumnName("descripcion").HasMaxLength(500);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .HasConstraintName("fk_lineas_asiento_cuenta")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CostCenter>()
            .WithMany()
            .HasForeignKey(e => e.CostCenterId)
            .HasConstraintName("fk_lineas_asiento_centro_costo")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Party>()
            .WithMany()
            .HasForeignKey(e => e.PartyId)
            .HasConstraintName("fk_lineas_asiento_tercero")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(e => e.CurrencyId)
            .HasConstraintName("fk_lineas_asiento_moneda")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.JournalEntryId, e.LineNumber }).IsUnique();
        builder.HasIndex(e => e.AccountId);
    }
}
