using Companies.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Companies.Configurations;

public sealed class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable("membresias");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.PublicId)
            .HasColumnName("id_publico")
            .HasDefaultValueSql("uuidv7()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(e => e.UserId).HasColumnName("usuario_id").IsRequired();
        builder.Property(e => e.CompanyId).HasColumnName("empresa_id").IsRequired();
        builder.Property(e => e.Role).HasColumnName("rol").HasColumnType("rol_membresia").IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("activo").IsRequired();
        builder.Property(e => e.GrantedAt).HasColumnName("otorgado_en").IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("fk_membresias_usuario")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(e => e.CompanyId)
            .HasConstraintName("fk_membresias_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.PublicId).IsUnique();
        builder.HasIndex(e => new { e.UserId, e.CompanyId }).IsUnique();
    }
}
