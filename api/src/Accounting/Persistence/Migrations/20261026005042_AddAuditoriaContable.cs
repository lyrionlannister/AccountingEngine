using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Accounting.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditoriaContable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auditoria_contable",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_publico = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    empresa_id = table.Column<long>(type: "bigint", nullable: false),
                    operacion = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    actor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ocurrido_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    notas = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    asiento_id = table.Column<long>(type: "bigint", nullable: true),
                    periodo_contable_id = table.Column<long>(type: "bigint", nullable: true),
                    solicitud_reapertura_id = table.Column<long>(type: "bigint", nullable: true),
                    saldo_cuenta_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoria_contable", x => x.id);
                    table.CheckConstraint("ck_auditoria_contable_una_referencia", "(CASE WHEN asiento_id IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN periodo_contable_id IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN solicitud_reapertura_id IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN saldo_cuenta_id IS NOT NULL THEN 1 ELSE 0 END) = 1");
                    table.ForeignKey(
                        name: "fk_auditoria_contable_asiento",
                        column: x => x.asiento_id,
                        principalTable: "asientos_contables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_auditoria_contable_periodo_contable",
                        column: x => x.periodo_contable_id,
                        principalTable: "periodos_contables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_auditoria_contable_saldo_cuenta",
                        column: x => x.saldo_cuenta_id,
                        principalTable: "saldos_cuenta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_auditoria_contable_solicitud_reapertura",
                        column: x => x.solicitud_reapertura_id,
                        principalTable: "solicitudes_reapertura_periodo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_contable_asiento_id",
                table: "auditoria_contable",
                column: "asiento_id");

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_contable_empresa_fecha",
                table: "auditoria_contable",
                columns: new[] { "empresa_id", "ocurrido_en" });

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_contable_id_publico",
                table: "auditoria_contable",
                column: "id_publico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_contable_periodo_contable_id",
                table: "auditoria_contable",
                column: "periodo_contable_id");

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_contable_saldo_cuenta_id",
                table: "auditoria_contable",
                column: "saldo_cuenta_id");

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_contable_solicitud_reapertura_id",
                table: "auditoria_contable",
                column: "solicitud_reapertura_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auditoria_contable");
        }
    }
}
