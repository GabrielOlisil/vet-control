using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAssinaturaAuditavelEComprovanteVacinas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComprovanteDocumentoPath",
                table: "AplicacoesVacina");

            migrationBuilder.AddColumn<string>(
                name: "LocalAplicacao",
                table: "AplicacoesVacina",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AssinadaEm",
                table: "AplicacoesVacina",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Assinante",
                table: "AplicacoesVacina",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssinanteCrmv",
                table: "AplicacoesVacina",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssinanteCrmvUf",
                table: "AplicacoesVacina",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataFabricacao",
                table: "AplicacoesVacina",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataValidade",
                table: "AplicacoesVacina",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HashAssinatura",
                table: "AplicacoesVacina",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetodoAssinatura",
                table: "AplicacoesVacina",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VeterinarioCrmv",
                table: "AplicacoesVacina",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VeterinarioCrmvUf",
                table: "AplicacoesVacina",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssinaturasAplicacoesVacina",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AplicacaoVacinaId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoEvento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NomeAssinante = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Crmv = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CrmvUf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    DataHora = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    HashConteudo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Metodo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssinaturasAplicacoesVacina", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssinaturasAplicacoesVacina_AplicacoesVacina_AplicacaoVacin~",
                        column: x => x.AplicacaoVacinaId,
                        principalTable: "AplicacoesVacina",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_HashAssinatura",
                table: "AplicacoesVacina",
                column: "HashAssinatura");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_StatusComprovante",
                table: "AplicacoesVacina",
                column: "StatusComprovante");

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturasAplicacoesVacina_AplicacaoVacinaId",
                table: "AssinaturasAplicacoesVacina",
                column: "AplicacaoVacinaId");

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturasAplicacoesVacina_DataHora",
                table: "AssinaturasAplicacoesVacina",
                column: "DataHora");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssinaturasAplicacoesVacina");

            migrationBuilder.DropIndex(
                name: "IX_AplicacoesVacina_HashAssinatura",
                table: "AplicacoesVacina");

            migrationBuilder.DropIndex(
                name: "IX_AplicacoesVacina_StatusComprovante",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "AssinadaEm",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "Assinante",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "AssinanteCrmv",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "AssinanteCrmvUf",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "DataFabricacao",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "DataValidade",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "HashAssinatura",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "MetodoAssinatura",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "VeterinarioCrmv",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "VeterinarioCrmvUf",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "LocalAplicacao",
                table: "AplicacoesVacina");

            migrationBuilder.AddColumn<string>(
                name: "ComprovanteDocumentoPath",
                table: "AplicacoesVacina",
                type: "text",
                nullable: true);
        }
    }
}
