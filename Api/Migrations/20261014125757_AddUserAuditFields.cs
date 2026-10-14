using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUser",
                table: "Vacinas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUser",
                table: "Vacinas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUser",
                table: "Racas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUser",
                table: "Racas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUser",
                table: "ProtocoloVacinalDoses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUser",
                table: "ProtocoloVacinalDoses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUser",
                table: "ProtocolosVacinais",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUser",
                table: "ProtocolosVacinais",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUser",
                table: "MovimentacoesAnimais",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUser",
                table: "MovimentacoesAnimais",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUser",
                table: "IdentificadorAnimal",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUser",
                table: "IdentificadorAnimal",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUser",
                table: "Especies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUser",
                table: "Especies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUser",
                table: "AplicacoesVacina",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUser",
                table: "AplicacoesVacina",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUser",
                table: "Animals",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUser",
                table: "Animals",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUser",
                table: "AnimalProtocolosVacinais",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUser",
                table: "AnimalProtocolosVacinais",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUser",
                table: "Vacinas");

            migrationBuilder.DropColumn(
                name: "UpdatedByUser",
                table: "Vacinas");

            migrationBuilder.DropColumn(
                name: "CreatedByUser",
                table: "Racas");

            migrationBuilder.DropColumn(
                name: "UpdatedByUser",
                table: "Racas");

            migrationBuilder.DropColumn(
                name: "CreatedByUser",
                table: "ProtocoloVacinalDoses");

            migrationBuilder.DropColumn(
                name: "UpdatedByUser",
                table: "ProtocoloVacinalDoses");

            migrationBuilder.DropColumn(
                name: "CreatedByUser",
                table: "ProtocolosVacinais");

            migrationBuilder.DropColumn(
                name: "UpdatedByUser",
                table: "ProtocolosVacinais");

            migrationBuilder.DropColumn(
                name: "CreatedByUser",
                table: "MovimentacoesAnimais");

            migrationBuilder.DropColumn(
                name: "UpdatedByUser",
                table: "MovimentacoesAnimais");

            migrationBuilder.DropColumn(
                name: "CreatedByUser",
                table: "IdentificadorAnimal");

            migrationBuilder.DropColumn(
                name: "UpdatedByUser",
                table: "IdentificadorAnimal");

            migrationBuilder.DropColumn(
                name: "CreatedByUser",
                table: "Especies");

            migrationBuilder.DropColumn(
                name: "UpdatedByUser",
                table: "Especies");

            migrationBuilder.DropColumn(
                name: "CreatedByUser",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "UpdatedByUser",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "CreatedByUser",
                table: "Animals");

            migrationBuilder.DropColumn(
                name: "UpdatedByUser",
                table: "Animals");

            migrationBuilder.DropColumn(
                name: "CreatedByUser",
                table: "AnimalProtocolosVacinais");

            migrationBuilder.DropColumn(
                name: "UpdatedByUser",
                table: "AnimalProtocolosVacinais");
        }
    }
}
