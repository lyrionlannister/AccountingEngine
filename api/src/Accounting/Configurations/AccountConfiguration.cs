using Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("cuentas");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.PublicId)
            .HasColumnName("id_publico")
            .HasDefaultValueSql("uuidv7()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(e => e.CompanyId).HasColumnName("empresa_id").IsRequired();
        builder.Property(e => e.Code).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Nature).HasColumnName("naturaleza").HasColumnType("naturaleza_cuenta").IsRequired();
        builder.Property(e => e.Type).HasColumnName("tipo_cuenta").HasColumnType("tipo_cuenta").IsRequired();
        builder.Property(e => e.ParentAccountId).HasColumnName("cuenta_padre_id");
        builder.Property(e => e.AllowsPosting).HasColumnName("permite_movimientos").IsRequired();
        builder.Property(e => e.RequiresCostCenter).HasColumnName("requiere_centro_costo").IsRequired();
        builder.Property(e => e.RequiresParty).HasColumnName("requiere_tercero").IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("activo").IsRequired();

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(e => e.ParentAccountId)
            .HasConstraintName("fk_cuentas_cuenta_padre")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.PublicId).IsUnique();
        builder.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
        builder.HasIndex(e => new { e.CompanyId, e.ParentAccountId });
    }
}
