using Auth.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Configurations;

public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("sesiones_usuario");

        // SessionId es la clave primaria: no hace falta un id interno separado — nada más
        // referencia una sesión por FK, y es el mismo valor que viaja en el claim "sid" del JWT.
        builder.HasKey(e => e.SessionId);
        builder.Property(e => e.SessionId).HasColumnName("sesion_id");

        builder.Property(e => e.UserId).HasColumnName("usuario_id").IsRequired();
        builder.Property(e => e.Ip).HasColumnName("ip").HasMaxLength(45).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("creado_en").IsRequired();
        builder.Property(e => e.LastSeenAt).HasColumnName("visto_en").IsRequired();

        // Respaldo de Postgres: solo se consulta por UserId cuando Redis no responde
        // (ver tasks/plan-auth.md, "Postgres es respaldo, no reflejo en tiempo real").
        builder.HasIndex(e => e.UserId);
    }
}
