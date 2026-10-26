using Auth.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Configurations;

public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("sesiones_usuario");

        builder.HasKey(e => e.SessionId);
        builder.Property(e => e.SessionId).HasColumnName("sesion_id");

        builder.Property(e => e.UserId).HasColumnName("usuario_id").IsRequired();
        builder.Property(e => e.Ip).HasColumnName("ip").HasMaxLength(45).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("creado_en").IsRequired();
        builder.Property(e => e.LastSeenAt).HasColumnName("visto_en").IsRequired();

        builder.HasIndex(e => e.UserId);
    }
}
