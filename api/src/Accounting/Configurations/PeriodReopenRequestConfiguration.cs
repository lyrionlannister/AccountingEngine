using Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Configurations;

public sealed class PeriodReopenRequestConfiguration : IEntityTypeConfiguration<PeriodReopenRequest>
{
    public void Configure(EntityTypeBuilder<PeriodReopenRequest> builder)
    {
        builder.ToTable("period_reopen_requests");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.PublicId)
            .HasColumnName("public_id")
            .HasDefaultValueSql("uuidv7()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(e => e.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(e => e.AccountingPeriodId).HasColumnName("accounting_period_id").IsRequired();
        builder.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(1000).IsRequired();
        builder.Property(e => e.RequestedBy).HasColumnName("requested_by").HasMaxLength(100).IsRequired();
        builder.Property(e => e.RequestedAt).HasColumnName("requested_at").IsRequired();
        builder.Property(e => e.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.ResolvedBy).HasColumnName("resolved_by").HasMaxLength(100);
        builder.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
        builder.Property(e => e.ResolutionNotes).HasColumnName("resolution_notes").HasMaxLength(1000);

        builder.HasOne<AccountingPeriod>()
            .WithMany()
            .HasForeignKey(e => e.AccountingPeriodId)
            .HasConstraintName("fk_period_reopen_requests_accounting_period")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.PublicId).IsUnique();
        builder.HasIndex(e => new { e.CompanyId, e.AccountingPeriodId, e.Status })
            .HasDatabaseName("ix_period_reopen_requests_company_period_status");
    }
}
