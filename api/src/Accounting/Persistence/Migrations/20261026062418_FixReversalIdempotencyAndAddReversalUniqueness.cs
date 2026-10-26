using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounting.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixReversalIdempotencyAndAddReversalUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_asientos_contables_asiento_revertido_id",
                table: "asientos_contables");

            migrationBuilder.DropIndex(
                name: "ix_asientos_contables_idempotencia_origen",
                table: "asientos_contables");

            migrationBuilder.CreateIndex(
                name: "ix_asientos_contables_idempotencia_origen",
                table: "asientos_contables",
                columns: new[] { "empresa_id", "modulo_origen", "referencia_origen" },
                unique: true,
                filter: "referencia_origen IS NOT NULL AND asiento_revertido_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_asientos_contables_reversion_unica",
                table: "asientos_contables",
                column: "asiento_revertido_id",
                unique: true,
                filter: "asiento_revertido_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_asientos_contables_idempotencia_origen",
                table: "asientos_contables");

            migrationBuilder.DropIndex(
                name: "ix_asientos_contables_reversion_unica",
                table: "asientos_contables");

            migrationBuilder.CreateIndex(
                name: "IX_asientos_contables_asiento_revertido_id",
                table: "asientos_contables",
                column: "asiento_revertido_id");

            migrationBuilder.CreateIndex(
                name: "ix_asientos_contables_idempotencia_origen",
                table: "asientos_contables",
                columns: new[] { "empresa_id", "modulo_origen", "referencia_origen" },
                unique: true,
                filter: "referencia_origen IS NOT NULL");
        }
    }
}
