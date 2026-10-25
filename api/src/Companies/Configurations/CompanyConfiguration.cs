using Companies.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Companies.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("empresas");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.PublicId)
            .HasColumnName("id_publico")
            .HasDefaultValueSql("uuidv7()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(e => e.TenantId).HasColumnName("inquilino_id").IsRequired();
        builder.Property(e => e.Name).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(e => e.CountryCode).HasColumnName("codigo_pais").HasColumnType("char(2)").IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("activo").IsRequired();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .HasConstraintName("fk_empresas_inquilino")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.PublicId).IsUnique();
        builder.HasIndex(e => e.TenantId);
    }
}
