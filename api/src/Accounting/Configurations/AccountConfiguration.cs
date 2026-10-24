using Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.PublicId)
            .HasColumnName("public_id")
            .HasDefaultValueSql("uuidv7()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(e => e.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(e => e.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Nature).HasColumnName("nature").HasColumnType("account_nature").IsRequired();
        builder.Property(e => e.Type).HasColumnName("account_type").HasColumnType("account_type").IsRequired();
        builder.Property(e => e.ParentAccountId).HasColumnName("parent_account_id");
        builder.Property(e => e.AllowsPosting).HasColumnName("allows_posting").IsRequired();
        builder.Property(e => e.RequiresCostCenter).HasColumnName("requires_cost_center").IsRequired();
        builder.Property(e => e.RequiresParty).HasColumnName("requires_party").IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(e => e.ParentAccountId)
            .HasConstraintName("fk_accounts_parent_account")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.PublicId).IsUnique();
        builder.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
        builder.HasIndex(e => new { e.CompanyId, e.ParentAccountId });
    }
}
