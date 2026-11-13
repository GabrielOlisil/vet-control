using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDistributedSessionsAndSignatureActor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssinanteUserId",
                table: "AssinaturasAplicacoesVacina",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServerSessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProtectedTicket = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturasAplicacoesVacina_AssinanteUserId",
                table: "AssinaturasAplicacoesVacina",
                column: "AssinanteUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerSessions_ExpiresAt",
                table: "ServerSessions",
                column: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerSessions");

            migrationBuilder.DropIndex(
                name: "IX_AssinaturasAplicacoesVacina_AssinanteUserId",
                table: "AssinaturasAplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "AssinanteUserId",
                table: "AssinaturasAplicacoesVacina");
        }
    }
}
