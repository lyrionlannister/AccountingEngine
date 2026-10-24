using System;
using Accounting.Domain.Enums;
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
                .Annotation("Npgsql:Enum:estado_asiento", "draft,posted,voided")
                .Annotation("Npgsql:Enum:estado_periodo", "open,closed")
                .Annotation("Npgsql:Enum:naturaleza_cuenta", "debit,credit")
                .Annotation("Npgsql:Enum:tipo_cuenta", "asset,liability,equity,revenue,expense");

            migrationBuilder.CreateTable(
                name: "centros_costo",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_publico = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    empresa_id = table.Column<long>(type: "bigint", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_centros_costo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cuentas",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_publico = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    empresa_id = table.Column<long>(type: "bigint", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    naturaleza = table.Column<AccountNature>(type: "naturaleza_cuenta", nullable: false),
                    tipo_cuenta = table.Column<AccountType>(type: "tipo_cuenta", nullable: false),
                    cuenta_padre_id = table.Column<long>(type: "bigint", nullable: true),
                    permite_movimientos = table.Column<bool>(type: "boolean", nullable: false),
                    requiere_centro_costo = table.Column<bool>(type: "boolean", nullable: false),
                    requiere_tercero = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuentas", x => x.id);
                    table.ForeignKey(
                        name: "fk_cuentas_cuenta_padre",
                        column: x => x.cuenta_padre_id,
                        principalTable: "cuentas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "monedas",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    codigo = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    decimales = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monedas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "periodos_contables",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_publico = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    empresa_id = table.Column<long>(type: "bigint", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<PeriodStatus>(type: "estado_periodo", nullable: false),
                    cerrado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cerrado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_periodos_contables", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "terceros",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_publico = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    empresa_id = table.Column<long>(type: "bigint", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_terceros", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipos_comprobante",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_publico = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    empresa_id = table.Column<long>(type: "bigint", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    modo_numeracion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_comprobante", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipos_cambio",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    moneda_origen_id = table.Column<long>(type: "bigint", nullable: false),
                    moneda_destino_id = table.Column<long>(type: "bigint", nullable: false),
                    tasa = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    fecha_vigencia = table.Column<DateOnly>(type: "date", nullable: false),
                    fuente = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_cambio", x => x.id);
                    table.ForeignKey(
                        name: "fk_tipos_cambio_moneda_destino",
                        column: x => x.moneda_destino_id,
                        principalTable: "monedas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tipos_cambio_moneda_origen",
                        column: x => x.moneda_origen_id,
                        principalTable: "monedas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "saldos_cuenta",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    periodo_contable_id = table.Column<long>(type: "bigint", nullable: false),
                    cuenta_id = table.Column<long>(type: "bigint", nullable: false),
                    total_debito = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_credito = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    calculado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saldos_cuenta", x => x.id);
                    table.ForeignKey(
                        name: "fk_saldos_cuenta_cuenta",
                        column: x => x.cuenta_id,
                        principalTable: "cuentas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_saldos_cuenta_periodo_contable",
                        column: x => x.periodo_contable_id,
                        principalTable: "periodos_contables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "saldos_iniciales",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    periodo_contable_id = table.Column<long>(type: "bigint", nullable: false),
                    cuenta_id = table.Column<long>(type: "bigint", nullable: false),
                    moneda_id = table.Column<long>(type: "bigint", nullable: false),
                    monto_debito = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    monto_credito = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saldos_iniciales", x => x.id);
                    table.CheckConstraint("ck_saldos_iniciales_debito_xor_credito", "(monto_debito > 0 AND monto_credito = 0) OR (monto_debito = 0 AND monto_credito > 0)");
                    table.ForeignKey(
                        name: "fk_saldos_iniciales_cuenta",
                        column: x => x.cuenta_id,
                        principalTable: "cuentas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_saldos_iniciales_moneda",
                        column: x => x.moneda_id,
                        principalTable: "monedas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_saldos_iniciales_periodo_contable",
                        column: x => x.periodo_contable_id,
                        principalTable: "periodos_contables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitudes_reapertura_periodo",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_publico = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    empresa_id = table.Column<long>(type: "bigint", nullable: false),
                    periodo_contable_id = table.Column<long>(type: "bigint", nullable: false),
                    motivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    solicitado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    solicitado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    resuelto_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    resuelto_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    notas_resolucion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitudes_reapertura_periodo", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitudes_reapertura_periodo_periodo_contable",
                        column: x => x.periodo_contable_id,
                        principalTable: "periodos_contables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asientos_contables",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_publico = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    empresa_id = table.Column<long>(type: "bigint", nullable: false),
                    tipo_comprobante_id = table.Column<long>(type: "bigint", nullable: false),
                    periodo_contable_id = table.Column<long>(type: "bigint", nullable: false),
                    numero_secuencia = table.Column<long>(type: "bigint", nullable: true),
                    estado = table.Column<JournalEntryStatus>(type: "estado_asiento", nullable: false),
                    fecha_asiento = table.Column<DateOnly>(type: "date", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    modulo_origen = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    referencia_origen = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    asiento_revertido_id = table.Column<long>(type: "bigint", nullable: true),
                    contabilizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    contabilizado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asientos_contables", x => x.id);
                    table.ForeignKey(
                        name: "fk_asientos_contables_asiento_revertido",
                        column: x => x.asiento_revertido_id,
                        principalTable: "asientos_contables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_asientos_contables_periodo_contable",
                        column: x => x.periodo_contable_id,
                        principalTable: "periodos_contables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_asientos_contables_tipo_comprobante",
                        column: x => x.tipo_comprobante_id,
                        principalTable: "tipos_comprobante",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lineas_asiento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    asiento_contable_id = table.Column<long>(type: "bigint", nullable: false),
                    numero_linea = table.Column<int>(type: "integer", nullable: false),
                    cuenta_id = table.Column<long>(type: "bigint", nullable: false),
                    centro_costo_id = table.Column<long>(type: "bigint", nullable: true),
                    tercero_id = table.Column<long>(type: "bigint", nullable: true),
                    moneda_id = table.Column<long>(type: "bigint", nullable: false),
                    monto_debito = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    monto_credito = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lineas_asiento", x => x.id);
                    table.CheckConstraint("ck_lineas_asiento_debito_xor_credito", "(monto_debito > 0 AND monto_credito = 0) OR (monto_debito = 0 AND monto_credito > 0)");
                    table.ForeignKey(
                        name: "fk_lineas_asiento_asiento_contable",
                        column: x => x.asiento_contable_id,
                        principalTable: "asientos_contables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lineas_asiento_centro_costo",
                        column: x => x.centro_costo_id,
                        principalTable: "centros_costo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lineas_asiento_cuenta",
                        column: x => x.cuenta_id,
                        principalTable: "cuentas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lineas_asiento_moneda",
                        column: x => x.moneda_id,
                        principalTable: "monedas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lineas_asiento_tercero",
                        column: x => x.tercero_id,
                        principalTable: "terceros",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_asientos_contables_asiento_revertido_id",
                table: "asientos_contables",
                column: "asiento_revertido_id");

            migrationBuilder.CreateIndex(
                name: "ix_asientos_contables_empresa_comprobante_secuencia",
                table: "asientos_contables",
                columns: new[] { "empresa_id", "tipo_comprobante_id", "numero_secuencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asientos_contables_empresa_periodo_estado",
                table: "asientos_contables",
                columns: new[] { "empresa_id", "periodo_contable_id", "estado" });

            migrationBuilder.CreateIndex(
                name: "IX_asientos_contables_id_publico",
                table: "asientos_contables",
                column: "id_publico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asientos_contables_idempotencia_origen",
                table: "asientos_contables",
                columns: new[] { "empresa_id", "modulo_origen", "referencia_origen" },
                unique: true,
                filter: "referencia_origen IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_asientos_contables_periodo_contable_id",
                table: "asientos_contables",
                column: "periodo_contable_id");

            migrationBuilder.CreateIndex(
                name: "IX_asientos_contables_tipo_comprobante_id",
                table: "asientos_contables",
                column: "tipo_comprobante_id");

            migrationBuilder.CreateIndex(
                name: "IX_centros_costo_empresa_id_codigo",
                table: "centros_costo",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_centros_costo_id_publico",
                table: "centros_costo",
                column: "id_publico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_cuenta_padre_id",
                table: "cuentas",
                column: "cuenta_padre_id");

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_empresa_id_codigo",
                table: "cuentas",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_empresa_id_cuenta_padre_id",
                table: "cuentas",
                columns: new[] { "empresa_id", "cuenta_padre_id" });

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_id_publico",
                table: "cuentas",
                column: "id_publico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lineas_asiento_asiento_contable_id_numero_linea",
                table: "lineas_asiento",
                columns: new[] { "asiento_contable_id", "numero_linea" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lineas_asiento_centro_costo_id",
                table: "lineas_asiento",
                column: "centro_costo_id");

            migrationBuilder.CreateIndex(
                name: "IX_lineas_asiento_cuenta_id",
                table: "lineas_asiento",
                column: "cuenta_id");

            migrationBuilder.CreateIndex(
                name: "IX_lineas_asiento_moneda_id",
                table: "lineas_asiento",
                column: "moneda_id");

            migrationBuilder.CreateIndex(
                name: "IX_lineas_asiento_tercero_id",
                table: "lineas_asiento",
                column: "tercero_id");

            migrationBuilder.CreateIndex(
                name: "IX_monedas_codigo",
                table: "monedas",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_periodos_contables_empresa_id_fecha_inicio_fecha_fin",
                table: "periodos_contables",
                columns: new[] { "empresa_id", "fecha_inicio", "fecha_fin" });

            migrationBuilder.CreateIndex(
                name: "IX_periodos_contables_empresa_id_nombre",
                table: "periodos_contables",
                columns: new[] { "empresa_id", "nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_periodos_contables_id_publico",
                table: "periodos_contables",
                column: "id_publico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_saldos_cuenta_cuenta_id",
                table: "saldos_cuenta",
                column: "cuenta_id");

            migrationBuilder.CreateIndex(
                name: "IX_saldos_cuenta_periodo_contable_id_cuenta_id",
                table: "saldos_cuenta",
                columns: new[] { "periodo_contable_id", "cuenta_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_saldos_iniciales_cuenta_id",
                table: "saldos_iniciales",
                column: "cuenta_id");

            migrationBuilder.CreateIndex(
                name: "IX_saldos_iniciales_moneda_id",
                table: "saldos_iniciales",
                column: "moneda_id");

            migrationBuilder.CreateIndex(
                name: "IX_saldos_iniciales_periodo_contable_id_cuenta_id",
                table: "saldos_iniciales",
                columns: new[] { "periodo_contable_id", "cuenta_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_solicitudes_reapertura_periodo_empresa_periodo_estado",
                table: "solicitudes_reapertura_periodo",
                columns: new[] { "empresa_id", "periodo_contable_id", "estado" });

            migrationBuilder.CreateIndex(
                name: "IX_solicitudes_reapertura_periodo_id_publico",
                table: "solicitudes_reapertura_periodo",
                column: "id_publico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_solicitudes_reapertura_periodo_periodo_contable_id",
                table: "solicitudes_reapertura_periodo",
                column: "periodo_contable_id");

            migrationBuilder.CreateIndex(
                name: "IX_terceros_empresa_id_codigo",
                table: "terceros",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_terceros_id_publico",
                table: "terceros",
                column: "id_publico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tipos_cambio_moneda_destino_id",
                table: "tipos_cambio",
                column: "moneda_destino_id");

            migrationBuilder.CreateIndex(
                name: "ix_tipos_cambio_par_fecha",
                table: "tipos_cambio",
                columns: new[] { "moneda_origen_id", "moneda_destino_id", "fecha_vigencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tipos_comprobante_empresa_id_codigo",
                table: "tipos_comprobante",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tipos_comprobante_id_publico",
                table: "tipos_comprobante",
                column: "id_publico",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lineas_asiento");

            migrationBuilder.DropTable(
                name: "saldos_cuenta");

            migrationBuilder.DropTable(
                name: "saldos_iniciales");

            migrationBuilder.DropTable(
                name: "solicitudes_reapertura_periodo");

            migrationBuilder.DropTable(
                name: "tipos_cambio");

            migrationBuilder.DropTable(
                name: "asientos_contables");

            migrationBuilder.DropTable(
                name: "centros_costo");

            migrationBuilder.DropTable(
                name: "terceros");

            migrationBuilder.DropTable(
                name: "cuentas");

            migrationBuilder.DropTable(
                name: "monedas");

            migrationBuilder.DropTable(
                name: "periodos_contables");

            migrationBuilder.DropTable(
                name: "tipos_comprobante");
        }
    }
}
