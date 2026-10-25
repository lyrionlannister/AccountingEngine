using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Companies.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_administradores_plataforma_usuario",
                table: "administradores_plataforma");

            migrationBuilder.DropForeignKey(
                name: "fk_membresias_usuario",
                table: "membresias");

            // La tabla "usuarios" NO se elimina: su ownership se traspasa a UsersDbContext
            // (módulo Users), que la conserva intacta en su propia InitialUsersSchema sin
            // volver a crearla. Ver tasks/plan-users.md — "traspaso de ownership", Task 7.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "fk_administradores_plataforma_usuario",
                table: "administradores_plataforma",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_membresias_usuario",
                table: "membresias",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
