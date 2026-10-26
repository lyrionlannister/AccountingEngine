using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain;

namespace Users.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.PublicId)
            .HasColumnName("id_publico")
            .HasDefaultValueSql("uuidv7()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(e => e.Email).HasColumnName("correo").HasMaxLength(320).IsRequired();
        builder.Property(e => e.PasswordHash).HasColumnName("contrasena_hash").IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("activo").IsRequired();

        builder.Property<string>("NormalizedEmail")
            .HasColumnName("correo_normalizado")
            .HasComputedColumnSql("lower(correo)", stored: true);

        builder.HasIndex(e => e.PublicId).IsUnique();
        builder.HasIndex("NormalizedEmail").IsUnique().HasDatabaseName("ix_usuarios_correo_normalizado");
    }
}
