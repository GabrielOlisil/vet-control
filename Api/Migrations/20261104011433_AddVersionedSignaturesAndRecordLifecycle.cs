using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionedSignaturesAndRecordLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Vacinas",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AlteradaEm",
                table: "AplicacoesVacina",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AlteradaPorUserId",
                table: "AplicacoesVacina",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AplicadorUserId",
                table: "AplicacoesVacina",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssinanteUserId",
                table: "AplicacoesVacina",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoAlteracao",
                table: "AplicacoesVacina",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RetificaAplicacaoVacinaId",
                table: "AplicacoesVacina",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SituacaoRegistro",
                table: "AplicacoesVacina",
                type: "text",
                nullable: false,
                defaultValue: "Ativo");

            migrationBuilder.AddColumn<string>(
                name: "SnapshotAssinado",
                table: "AplicacoesVacina",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VersaoAssinatura",
                table: "AplicacoesVacina",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "VeterinarioResponsavelUserId",
                table: "AplicacoesVacina",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "AplicacoesVacina",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_RetificaAplicacaoVacinaId",
                table: "AplicacoesVacina",
                column: "RetificaAplicacaoVacinaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_SituacaoRegistro",
                table: "AplicacoesVacina",
                column: "SituacaoRegistro");

            migrationBuilder.AddForeignKey(
                name: "FK_AplicacoesVacina_AplicacoesVacina_RetificaAplicacaoVacinaId",
                table: "AplicacoesVacina",
                column: "RetificaAplicacaoVacinaId",
                principalTable: "AplicacoesVacina",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AplicacoesVacina_AplicacoesVacina_RetificaAplicacaoVacinaId",
                table: "AplicacoesVacina");

            migrationBuilder.DropIndex(
                name: "IX_AplicacoesVacina_RetificaAplicacaoVacinaId",
                table: "AplicacoesVacina");

            migrationBuilder.DropIndex(
                name: "IX_AplicacoesVacina_SituacaoRegistro",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Vacinas");

            migrationBuilder.DropColumn(
                name: "AlteradaEm",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "AlteradaPorUserId",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "AplicadorUserId",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "AssinanteUserId",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "MotivoAlteracao",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "RetificaAplicacaoVacinaId",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "SituacaoRegistro",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "SnapshotAssinado",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "VersaoAssinatura",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "VeterinarioResponsavelUserId",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "AplicacoesVacina");
        }
    }
}
