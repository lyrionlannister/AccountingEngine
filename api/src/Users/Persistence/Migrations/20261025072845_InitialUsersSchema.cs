using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Users.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialUsersSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La tabla "usuarios" ya existe — fue creada por Companies' InitialCompaniesSchema y
            // su ownership se traspasó aquí (ver Companies' RemoveUserOwnership, que solo quita
            // los FKs cruzando el límite de módulo, nunca la tabla). Ver tasks/plan-users.md.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
