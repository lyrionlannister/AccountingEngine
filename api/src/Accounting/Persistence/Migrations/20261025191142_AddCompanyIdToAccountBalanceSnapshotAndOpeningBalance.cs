using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounting.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyIdToAccountBalanceSnapshotAndOpeningBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_saldos_iniciales_periodo_contable_id_cuenta_id",
                table: "saldos_iniciales");

            migrationBuilder.DropIndex(
                name: "IX_saldos_cuenta_periodo_contable_id_cuenta_id",
                table: "saldos_cuenta");

            migrationBuilder.AddColumn<long>(
                name: "empresa_id",
                table: "saldos_iniciales",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "empresa_id",
                table: "saldos_cuenta",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_saldos_iniciales_empresa_id_periodo_contable_id_cuenta_id",
                table: "saldos_iniciales",
                columns: new[] { "empresa_id", "periodo_contable_id", "cuenta_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_saldos_iniciales_periodo_contable_id",
                table: "saldos_iniciales",
                column: "periodo_contable_id");

            migrationBuilder.CreateIndex(
                name: "IX_saldos_cuenta_empresa_id_periodo_contable_id_cuenta_id",
                table: "saldos_cuenta",
                columns: new[] { "empresa_id", "periodo_contable_id", "cuenta_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_saldos_cuenta_periodo_contable_id",
                table: "saldos_cuenta",
                column: "periodo_contable_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_saldos_iniciales_empresa_id_periodo_contable_id_cuenta_id",
                table: "saldos_iniciales");

            migrationBuilder.DropIndex(
                name: "IX_saldos_iniciales_periodo_contable_id",
                table: "saldos_iniciales");

            migrationBuilder.DropIndex(
                name: "IX_saldos_cuenta_empresa_id_periodo_contable_id_cuenta_id",
                table: "saldos_cuenta");

            migrationBuilder.DropIndex(
                name: "IX_saldos_cuenta_periodo_contable_id",
                table: "saldos_cuenta");

            migrationBuilder.DropColumn(
                name: "empresa_id",
                table: "saldos_iniciales");

            migrationBuilder.DropColumn(
                name: "empresa_id",
                table: "saldos_cuenta");

            migrationBuilder.CreateIndex(
                name: "IX_saldos_iniciales_periodo_contable_id_cuenta_id",
                table: "saldos_iniciales",
                columns: new[] { "periodo_contable_id", "cuenta_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_saldos_cuenta_periodo_contable_id_cuenta_id",
                table: "saldos_cuenta",
                columns: new[] { "periodo_contable_id", "cuenta_id" },
                unique: true);
        }
    }
}
