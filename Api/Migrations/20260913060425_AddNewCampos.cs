using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddNewCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "DataProximaDose",
                table: "AplicacoesVacina",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<bool>(
                name: "CicloFinalizado",
                table: "AplicacoesVacina",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ComprovanteDocumentoPath",
                table: "AplicacoesVacina",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusComprovante",
                table: "AplicacoesVacina",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_DataAplicacao",
                table: "AplicacoesVacina",
                column: "DataAplicacao");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_DataProximaDose",
                table: "AplicacoesVacina",
                column: "DataProximaDose");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AplicacoesVacina_DataAplicacao",
                table: "AplicacoesVacina");

            migrationBuilder.DropIndex(
                name: "IX_AplicacoesVacina_DataProximaDose",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "CicloFinalizado",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "ComprovanteDocumentoPath",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "StatusComprovante",
                table: "AplicacoesVacina");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DataProximaDose",
                table: "AplicacoesVacina",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);
        }
    }
}
