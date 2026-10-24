using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounting.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TightenConstraintsAndIndexNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_opening_balances_debit_xor_credit",
                table: "opening_balances");

            migrationBuilder.DropCheckConstraint(
                name: "ck_journal_entry_lines_debit_xor_credit",
                table: "journal_entry_lines");

            migrationBuilder.DropIndex(
                name: "IX_exchange_rates_from_currency_id_to_currency_id_effective_da~",
                table: "exchange_rates");

            migrationBuilder.RenameIndex(
                name: "IX_period_reopen_requests_company_id_accounting_period_id_stat~",
                table: "period_reopen_requests",
                newName: "ix_period_reopen_requests_company_period_status");

            migrationBuilder.RenameIndex(
                name: "IX_journal_entries_company_id_accounting_period_id_journal_ent~",
                table: "journal_entries",
                newName: "ix_journal_entries_company_period_status");

            migrationBuilder.AddCheckConstraint(
                name: "ck_opening_balances_debit_xor_credit",
                table: "opening_balances",
                sql: "(debit_amount > 0 AND credit_amount = 0) OR (debit_amount = 0 AND credit_amount > 0)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_journal_entry_lines_debit_xor_credit",
                table: "journal_entry_lines",
                sql: "(debit_amount > 0 AND credit_amount = 0) OR (debit_amount = 0 AND credit_amount > 0)");

            migrationBuilder.CreateIndex(
                name: "ix_exchange_rates_pair_date",
                table: "exchange_rates",
                columns: new[] { "from_currency_id", "to_currency_id", "effective_date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_opening_balances_debit_xor_credit",
                table: "opening_balances");

            migrationBuilder.DropCheckConstraint(
                name: "ck_journal_entry_lines_debit_xor_credit",
                table: "journal_entry_lines");

            migrationBuilder.DropIndex(
                name: "ix_exchange_rates_pair_date",
                table: "exchange_rates");

            migrationBuilder.RenameIndex(
                name: "ix_period_reopen_requests_company_period_status",
                table: "period_reopen_requests",
                newName: "IX_period_reopen_requests_company_id_accounting_period_id_stat~");

            migrationBuilder.RenameIndex(
                name: "ix_journal_entries_company_period_status",
                table: "journal_entries",
                newName: "IX_journal_entries_company_id_accounting_period_id_journal_ent~");

            migrationBuilder.AddCheckConstraint(
                name: "ck_opening_balances_debit_xor_credit",
                table: "opening_balances",
                sql: "debit_amount = 0 OR credit_amount = 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_journal_entry_lines_debit_xor_credit",
                table: "journal_entry_lines",
                sql: "debit_amount = 0 OR credit_amount = 0");

            migrationBuilder.CreateIndex(
                name: "IX_exchange_rates_from_currency_id_to_currency_id_effective_da~",
                table: "exchange_rates",
                columns: new[] { "from_currency_id", "to_currency_id", "effective_date" });
        }
    }
}
