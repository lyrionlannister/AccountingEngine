using Companies.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Companies.Configurations;

public sealed class PlatformAdministratorConfiguration : IEntityTypeConfiguration<PlatformAdministrator>
{
    public void Configure(EntityTypeBuilder<PlatformAdministrator> builder)
    {
        builder.ToTable("administradores_plataforma");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.PublicId)
            .HasColumnName("id_publico")
            .HasDefaultValueSql("uuidv7()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(e => e.UserId).HasColumnName("usuario_id").IsRequired();
        builder.Property(e => e.GrantedAt).HasColumnName("otorgado_en").IsRequired();

        // UserId es un long plano, sin FK de Postgres hacia la tabla usuarios (propiedad del
        // módulo Users) — mismo patrón que Accounting.CompanyId, ver tasks/plan-users.md.
        builder.HasIndex(e => e.PublicId).IsUnique();
        builder.HasIndex(e => e.UserId).IsUnique();
    }
}
