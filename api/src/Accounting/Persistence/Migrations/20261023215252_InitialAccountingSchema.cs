using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Accounting.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAccountingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:account_nature", "debit,credit")
                .Annotation("Npgsql:Enum:account_type", "asset,liability,equity,revenue,expense")
                .Annotation("Npgsql:Enum:journal_entry_status", "draft,posted,voided")
                .Annotation("Npgsql:Enum:period_status", "open,closed");

            migrationBuilder.CreateTable(
                name: "accounting_periods",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    company_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    period_status = table.Column<int>(type: "period_status", nullable: false),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    closed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounting_periods", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    company_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    nature = table.Column<int>(type: "account_nature", nullable: false),
                    account_type = table.Column<int>(type: "account_type", nullable: false),
                    parent_account_id = table.Column<long>(type: "bigint", nullable: true),
                    allows_posting = table.Column<bool>(type: "boolean", nullable: false),
                    requires_cost_center = table.Column<bool>(type: "boolean", nullable: false),
                    requires_party = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_accounts_parent_account",
                        column: x => x.parent_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cost_centers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    company_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cost_centers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "currencies",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    decimal_places = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currencies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parties",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    company_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parties", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "voucher_types",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    company_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    numbering_mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voucher_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "period_reopen_requests",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    company_id = table.Column<long>(type: "bigint", nullable: false),
                    accounting_period_id = table.Column<long>(type: "bigint", nullable: false),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    requested_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    resolved_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    resolution_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_period_reopen_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_period_reopen_requests_accounting_period",
                        column: x => x.accounting_period_id,
                        principalTable: "accounting_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "account_balance_snapshots",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    accounting_period_id = table.Column<long>(type: "bigint", nullable: false),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    debit_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    credit_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    calculated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_balance_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "fk_account_balance_snapshots_account",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_account_balance_snapshots_accounting_period",
                        column: x => x.accounting_period_id,
                        principalTable: "accounting_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exchange_rates",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    from_currency_id = table.Column<long>(type: "bigint", nullable: false),
                    to_currency_id = table.Column<long>(type: "bigint", nullable: false),
                    rate = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    effective_date = table.Column<DateOnly>(type: "date", nullable: false),
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exchange_rates", x => x.id);
                    table.ForeignKey(
                        name: "fk_exchange_rates_from_currency",
                        column: x => x.from_currency_id,
                        principalTable: "currencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_exchange_rates_to_currency",
                        column: x => x.to_currency_id,
                        principalTable: "currencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "opening_balances",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    accounting_period_id = table.Column<long>(type: "bigint", nullable: false),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    currency_id = table.Column<long>(type: "bigint", nullable: false),
                    debit_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    credit_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opening_balances", x => x.id);
                    table.CheckConstraint("ck_opening_balances_debit_xor_credit", "debit_amount = 0 OR credit_amount = 0");
                    table.ForeignKey(
                        name: "fk_opening_balances_account",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_opening_balances_accounting_period",
                        column: x => x.accounting_period_id,
                        principalTable: "accounting_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_opening_balances_currency",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "journal_entries",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    company_id = table.Column<long>(type: "bigint", nullable: false),
                    voucher_type_id = table.Column<long>(type: "bigint", nullable: false),
                    accounting_period_id = table.Column<long>(type: "bigint", nullable: false),
                    sequence_number = table.Column<long>(type: "bigint", nullable: true),
                    journal_entry_status = table.Column<int>(type: "journal_entry_status", nullable: false),
                    entry_date = table.Column<DateOnly>(type: "date", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    source_module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    source_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reversal_of_entry_id = table.Column<long>(type: "bigint", nullable: true),
                    posted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    posted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journal_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_journal_entries_accounting_period",
                        column: x => x.accounting_period_id,
                        principalTable: "accounting_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_journal_entries_reversal_of_entry",
                        column: x => x.reversal_of_entry_id,
                        principalTable: "journal_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_journal_entries_voucher_type",
                        column: x => x.voucher_type_id,
                        principalTable: "voucher_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "journal_entry_lines",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    journal_entry_id = table.Column<long>(type: "bigint", nullable: false),
                    line_number = table.Column<int>(type: "integer", nullable: false),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    cost_center_id = table.Column<long>(type: "bigint", nullable: true),
                    party_id = table.Column<long>(type: "bigint", nullable: true),
                    currency_id = table.Column<long>(type: "bigint", nullable: false),
                    debit_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    credit_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journal_entry_lines", x => x.id);
                    table.CheckConstraint("ck_journal_entry_lines_debit_xor_credit", "debit_amount = 0 OR credit_amount = 0");
                    table.ForeignKey(
                        name: "fk_journal_entry_lines_account",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_journal_entry_lines_cost_center",
                        column: x => x.cost_center_id,
                        principalTable: "cost_centers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_journal_entry_lines_currency",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_journal_entry_lines_journal_entry",
                        column: x => x.journal_entry_id,
                        principalTable: "journal_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_journal_entry_lines_party",
                        column: x => x.party_id,
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_balance_snapshots_account_id",
                table: "account_balance_snapshots",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_account_balance_snapshots_accounting_period_id_account_id",
                table: "account_balance_snapshots",
                columns: new[] { "accounting_period_id", "account_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accounting_periods_company_id_name",
                table: "accounting_periods",
                columns: new[] { "company_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accounting_periods_company_id_start_date_end_date",
                table: "accounting_periods",
                columns: new[] { "company_id", "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "IX_accounting_periods_public_id",
                table: "accounting_periods",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accounts_company_id_code",
                table: "accounts",
                columns: new[] { "company_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accounts_company_id_parent_account_id",
                table: "accounts",
                columns: new[] { "company_id", "parent_account_id" });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_parent_account_id",
                table: "accounts",
                column: "parent_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_accounts_public_id",
                table: "accounts",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cost_centers_company_id_code",
                table: "cost_centers",
                columns: new[] { "company_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cost_centers_public_id",
                table: "cost_centers",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_currencies_code",
                table: "currencies",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exchange_rates_from_currency_id_to_currency_id_effective_da~",
                table: "exchange_rates",
                columns: new[] { "from_currency_id", "to_currency_id", "effective_date" });

            migrationBuilder.CreateIndex(
                name: "IX_exchange_rates_to_currency_id",
                table: "exchange_rates",
                column: "to_currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_accounting_period_id",
                table: "journal_entries",
                column: "accounting_period_id");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_company_id_accounting_period_id_journal_ent~",
                table: "journal_entries",
                columns: new[] { "company_id", "accounting_period_id", "journal_entry_status" });

            migrationBuilder.CreateIndex(
                name: "ix_journal_entries_company_voucher_sequence",
                table: "journal_entries",
                columns: new[] { "company_id", "voucher_type_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_public_id",
                table: "journal_entries",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_reversal_of_entry_id",
                table: "journal_entries",
                column: "reversal_of_entry_id");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_voucher_type_id",
                table: "journal_entries",
                column: "voucher_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_lines_account_id",
                table: "journal_entry_lines",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_lines_cost_center_id",
                table: "journal_entry_lines",
                column: "cost_center_id");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_lines_currency_id",
                table: "journal_entry_lines",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_lines_journal_entry_id_line_number",
                table: "journal_entry_lines",
                columns: new[] { "journal_entry_id", "line_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_lines_party_id",
                table: "journal_entry_lines",
                column: "party_id");

            migrationBuilder.CreateIndex(
                name: "IX_opening_balances_account_id",
                table: "opening_balances",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_opening_balances_accounting_period_id_account_id",
                table: "opening_balances",
                columns: new[] { "accounting_period_id", "account_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_opening_balances_currency_id",
                table: "opening_balances",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_parties_company_id_code",
                table: "parties",
                columns: new[] { "company_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parties_public_id",
                table: "parties",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_period_reopen_requests_accounting_period_id",
                table: "period_reopen_requests",
                column: "accounting_period_id");

            migrationBuilder.CreateIndex(
                name: "IX_period_reopen_requests_company_id_accounting_period_id_stat~",
                table: "period_reopen_requests",
                columns: new[] { "company_id", "accounting_period_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_period_reopen_requests_public_id",
                table: "period_reopen_requests",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voucher_types_company_id_code",
                table: "voucher_types",
                columns: new[] { "company_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voucher_types_public_id",
                table: "voucher_types",
                column: "public_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_balance_snapshots");

            migrationBuilder.DropTable(
                name: "exchange_rates");

            migrationBuilder.DropTable(
                name: "journal_entry_lines");

            migrationBuilder.DropTable(
                name: "opening_balances");

            migrationBuilder.DropTable(
                name: "period_reopen_requests");

            migrationBuilder.DropTable(
                name: "cost_centers");

            migrationBuilder.DropTable(
                name: "journal_entries");

            migrationBuilder.DropTable(
                name: "parties");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "currencies");

            migrationBuilder.DropTable(
                name: "accounting_periods");

            migrationBuilder.DropTable(
                name: "voucher_types");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:account_nature", "debit,credit")
                .OldAnnotation("Npgsql:Enum:account_type", "asset,liability,equity,revenue,expense")
                .OldAnnotation("Npgsql:Enum:journal_entry_status", "draft,posted,voided")
                .OldAnnotation("Npgsql:Enum:period_status", "open,closed");
        }
    }
}
