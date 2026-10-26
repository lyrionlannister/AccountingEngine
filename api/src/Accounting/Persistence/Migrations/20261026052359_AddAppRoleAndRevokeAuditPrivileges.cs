using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounting.Persistence.Migrations
{
    /// <inheritdoc />
    // Rol de aplicacion separado del rol propietario/migraciones (tasks/plan-auditoria.md
    // seccion 3.3): sin esto, "revocar UPDATE/DELETE sobre auditoria_contable al rol de la
    // aplicacion" no es real — el propietario de una tabla siempre puede editarla/borrarla sin
    // importar los REVOKE. Esta migracion sigue corriendo con el rol propietario (POSTGRES_USER);
    // el rol nuevo (motor_contable_app) es el que usa la app en runtime desde Fase 3 en adelante.
    public partial class AddAppRoleAndRevokeAuditPrivileges : Migration
    {
        private const string AppRole = "motor_contable_app";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var password = Environment.GetEnvironmentVariable("ACCOUNTING_APP_DB_PASSWORD")
                ?? throw new InvalidOperationException(
                    $"La variable de entorno 'ACCOUNTING_APP_DB_PASSWORD' es obligatoria para crear/actualizar el rol " +
                    $"'{AppRole}' (tasks/plan-auditoria.md seccion 3.3). No se hardcodea la contrasena en la migracion.");

            // CREATE ROLE/ALTER ROLE no son instrucciones validas dentro de un bloque PL/pgSQL
            // sin EXECUTE — por eso van envueltas en format()/EXECUTE, no como SQL directo.
            var escapedPassword = password.Replace("'", "''");

            migrationBuilder.Sql($@"
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = '{AppRole}') THEN
        EXECUTE format('CREATE ROLE {AppRole} LOGIN PASSWORD %L', '{escapedPassword}');
    ELSE
        EXECUTE format('ALTER ROLE {AppRole} LOGIN PASSWORD %L', '{escapedPassword}');
    END IF;
END
$$;

DO $$
BEGIN
    EXECUTE format('GRANT CONNECT ON DATABASE %I TO {AppRole}', current_database());
END
$$;

GRANT USAGE ON SCHEMA public TO {AppRole};
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO {AppRole};
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO {AppRole};

-- Para que una tabla creada por una migracion futura (bajo el rol propietario actual) herede
-- estos privilegios sin otra migracion de roles.
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO {AppRole};
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT USAGE, SELECT ON SEQUENCES TO {AppRole};

-- La garantia real de esta fase: un REVOKE puntual sobre una tabla siempre gana sobre el GRANT
-- generico de arriba, sin importar el orden en que se hayan otorgado.
REVOKE UPDATE, DELETE ON auditoria_contable FROM {AppRole};
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No hace DROP ROLE si la app sigue conectada con este rol en el momento del
            // rollback — Postgres rechaza borrar un rol con sesiones activas o privilegios
            // pendientes de revocar en otras bases. Desconectar la app antes de revertir esta
            // migracion en un entorno real (documentado aqui, no oculto).
            migrationBuilder.Sql($@"
ALTER DEFAULT PRIVILEGES IN SCHEMA public REVOKE SELECT, INSERT, UPDATE, DELETE ON TABLES FROM {AppRole};
ALTER DEFAULT PRIVILEGES IN SCHEMA public REVOKE USAGE, SELECT ON SEQUENCES FROM {AppRole};
REVOKE ALL PRIVILEGES ON ALL TABLES IN SCHEMA public FROM {AppRole};
REVOKE ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public FROM {AppRole};
REVOKE USAGE ON SCHEMA public FROM {AppRole};

DO $$
BEGIN
    EXECUTE format('REVOKE CONNECT ON DATABASE %I FROM {AppRole}', current_database());
END
$$;

DROP ROLE IF EXISTS {AppRole};
");
        }
    }
}
