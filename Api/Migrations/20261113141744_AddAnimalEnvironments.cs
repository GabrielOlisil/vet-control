using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAnimalEnvironments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AmbienteDestinoId",
                table: "MovimentacoesAnimais",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AmbienteOrigemId",
                table: "MovimentacoesAnimais",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AmbienteId",
                table: "Animals",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Ambientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    NomeNormalizado = table.Column<string>(type: "text", nullable: false, computedColumnSql: "lower(btrim(\"Nome\"))", stored: true),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    CreationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUser = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUser = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ambientes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesAnimais_AmbienteDestinoId",
                table: "MovimentacoesAnimais",
                column: "AmbienteDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesAnimais_AmbienteOrigemId",
                table: "MovimentacoesAnimais",
                column: "AmbienteOrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_Animals_AmbienteId",
                table: "Animals",
                column: "AmbienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Ambientes_Nome",
                table: "Ambientes",
                column: "Nome")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Ambientes_NomeNormalizado",
                table: "Ambientes",
                column: "NomeNormalizado",
                unique: true);

            migrationBuilder.Sql("""
                WITH locais AS (
                    SELECT "LoteOuPasto" AS nome FROM "Animals"
                    UNION ALL SELECT "LocalOrigem" FROM "MovimentacoesAnimais"
                    UNION ALL SELECT "LocalDestino" FROM "MovimentacoesAnimais"
                ), catalogo AS (
                    SELECT min(btrim(nome)) AS nome, lower(btrim(nome)) AS chave
                    FROM locais WHERE nullif(btrim(nome), '') IS NOT NULL
                    GROUP BY lower(btrim(nome))
                )
                INSERT INTO "Ambientes" ("Id", "Nome", "Tipo", "CreationDateTime")
                SELECT gen_random_uuid(), nome,
                    CASE
                        WHEN chave ~ '^pasto([[:space:]]|$)' THEN 'Pasto'
                        WHEN chave ~ '^piquete([[:space:]]|$)' THEN 'Piquete'
                        WHEN chave ~ '^ba[ií]a([[:space:]]|$)' THEN 'Baia'
                        WHEN chave ~ '^curral([[:space:]]|$)' THEN 'Curral'
                        ELSE 'Outro'
                    END, now()
                FROM catalogo
                ON CONFLICT ("NomeNormalizado") DO NOTHING;

                UPDATE "Animals" animal SET "AmbienteId" = ambiente."Id"
                FROM "Ambientes" ambiente WHERE ambiente."NomeNormalizado" = lower(btrim(animal."LoteOuPasto"));

                UPDATE "MovimentacoesAnimais" movimento SET "AmbienteOrigemId" = ambiente."Id"
                FROM "Ambientes" ambiente WHERE ambiente."NomeNormalizado" = lower(btrim(movimento."LocalOrigem"));

                UPDATE "MovimentacoesAnimais" movimento SET "AmbienteDestinoId" = ambiente."Id"
                FROM "Ambientes" ambiente WHERE ambiente."NomeNormalizado" = lower(btrim(movimento."LocalDestino"));
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Animals_Ambientes_AmbienteId",
                table: "Animals",
                column: "AmbienteId",
                principalTable: "Ambientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacoesAnimais_Ambientes_AmbienteDestinoId",
                table: "MovimentacoesAnimais",
                column: "AmbienteDestinoId",
                principalTable: "Ambientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacoesAnimais_Ambientes_AmbienteOrigemId",
                table: "MovimentacoesAnimais",
                column: "AmbienteOrigemId",
                principalTable: "Ambientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Animals_Ambientes_AmbienteId",
                table: "Animals");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacoesAnimais_Ambientes_AmbienteDestinoId",
                table: "MovimentacoesAnimais");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacoesAnimais_Ambientes_AmbienteOrigemId",
                table: "MovimentacoesAnimais");

            migrationBuilder.DropTable(
                name: "Ambientes");

            migrationBuilder.DropIndex(
                name: "IX_MovimentacoesAnimais_AmbienteDestinoId",
                table: "MovimentacoesAnimais");

            migrationBuilder.DropIndex(
                name: "IX_MovimentacoesAnimais_AmbienteOrigemId",
                table: "MovimentacoesAnimais");

            migrationBuilder.DropIndex(
                name: "IX_Animals_AmbienteId",
                table: "Animals");

            migrationBuilder.DropColumn(
                name: "AmbienteDestinoId",
                table: "MovimentacoesAnimais");

            migrationBuilder.DropColumn(
                name: "AmbienteOrigemId",
                table: "MovimentacoesAnimais");

            migrationBuilder.DropColumn(
                name: "AmbienteId",
                table: "Animals");
        }
    }
}
