using System;
using Companies.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Companies.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCompaniesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:rol_membresia", "admin,accountant,viewer");

            migrationBuilder.CreateTable(
                name: "inquilinos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_publico = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inquilinos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_publico = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    correo = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    contrasena_hash = table.Column<string>(type: "text", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    correo_normalizado = table.Column<string>(type: "text", nullable: true, computedColumnSql: "lower(correo)", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "empresas",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_publico = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    inquilino_id = table.Column<long>(type: "bigint", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    codigo_pais = table.Column<string>(type: "char(2)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresas", x => x.id);
                    table.ForeignKey(
                        name: "fk_empresas_inquilino",
                        column: x => x.inquilino_id,
                        principalTable: "inquilinos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "administradores_plataforma",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_publico = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    usuario_id = table.Column<long>(type: "bigint", nullable: false),
                    otorgado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_administradores_plataforma", x => x.id);
                    table.ForeignKey(
                        name: "fk_administradores_plataforma_usuario",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "membresias",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_publico = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    usuario_id = table.Column<long>(type: "bigint", nullable: false),
                    empresa_id = table.Column<long>(type: "bigint", nullable: false),
                    rol = table.Column<CompanyMembershipRole>(type: "rol_membresia", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    otorgado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membresias", x => x.id);
                    table.ForeignKey(
                        name: "fk_membresias_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_membresias_usuario",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_administradores_plataforma_id_publico",
                table: "administradores_plataforma",
                column: "id_publico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_administradores_plataforma_usuario_id",
                table: "administradores_plataforma",
                column: "usuario_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_empresas_id_publico",
                table: "empresas",
                column: "id_publico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_empresas_inquilino_id",
                table: "empresas",
                column: "inquilino_id");

            migrationBuilder.CreateIndex(
                name: "IX_inquilinos_id_publico",
                table: "inquilinos",
                column: "id_publico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_membresias_empresa_id",
                table: "membresias",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "IX_membresias_id_publico",
                table: "membresias",
                column: "id_publico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_membresias_usuario_id_empresa_id",
                table: "membresias",
                columns: new[] { "usuario_id", "empresa_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_correo_normalizado",
                table: "usuarios",
                column: "correo_normalizado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_id_publico",
                table: "usuarios",
                column: "id_publico",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "administradores_plataforma");

            migrationBuilder.DropTable(
                name: "membresias");

            migrationBuilder.DropTable(
                name: "empresas");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "inquilinos");
        }
    }
}
