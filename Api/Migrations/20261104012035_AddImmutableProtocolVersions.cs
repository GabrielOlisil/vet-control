using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddImmutableProtocolVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FamiliaId",
                table: "ProtocolosVacinais",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<int>(
                name: "Versao",
                table: "ProtocolosVacinais",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "VersaoAtual",
                table: "ProtocolosVacinais",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "ProtocolosVacinais",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolosVacinais_FamiliaId",
                table: "ProtocolosVacinais",
                column: "FamiliaId",
                unique: true,
                filter: "\"VersaoAtual\"");

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolosVacinais_FamiliaId_Versao",
                table: "ProtocolosVacinais",
                columns: new[] { "FamiliaId", "Versao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnimalProtocolosVacinais_AnimalId_ProtocoloVacinalId",
                table: "AnimalProtocolosVacinais",
                columns: new[] { "AnimalId", "ProtocoloVacinalId" },
                unique: true,
                filter: "\"Ativo\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProtocolosVacinais_FamiliaId",
                table: "ProtocolosVacinais");

            migrationBuilder.DropIndex(
                name: "IX_ProtocolosVacinais_FamiliaId_Versao",
                table: "ProtocolosVacinais");

            migrationBuilder.DropIndex(
                name: "IX_AnimalProtocolosVacinais_AnimalId_ProtocoloVacinalId",
                table: "AnimalProtocolosVacinais");

            migrationBuilder.DropColumn(
                name: "FamiliaId",
                table: "ProtocolosVacinais");

            migrationBuilder.DropColumn(
                name: "Versao",
                table: "ProtocolosVacinais");

            migrationBuilder.DropColumn(
                name: "VersaoAtual",
                table: "ProtocolosVacinais");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "ProtocolosVacinais");
        }
    }
}
