using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProtocolosVacinaisEMovimentacoesAnimais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AnimalProtocoloVacinalId",
                table: "AplicacoesVacina",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProtocoloVacinalDoseId",
                table: "AplicacoesVacina",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MovimentacoesAnimais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnimalId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataMovimentacao = table.Column<DateOnly>(type: "date", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    LocalOrigem = table.Column<string>(type: "text", nullable: true),
                    LocalDestino = table.Column<string>(type: "text", nullable: true),
                    Motivo = table.Column<string>(type: "text", nullable: true),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    CreationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesAnimais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimentacoesAnimais_Animals_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProtocolosVacinais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    Finalidade = table.Column<string>(type: "text", nullable: true),
                    EspecieId = table.Column<Guid>(type: "uuid", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    IdadeMinimaDias = table.Column<int>(type: "integer", nullable: true),
                    IdadeMaximaDias = table.Column<int>(type: "integer", nullable: true),
                    CreationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtocolosVacinais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtocolosVacinais_Especies_EspecieId",
                        column: x => x.EspecieId,
                        principalTable: "Especies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AnimalProtocolosVacinais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnimalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProtocoloVacinalId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    CreationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalProtocolosVacinais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnimalProtocolosVacinais_Animals_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnimalProtocolosVacinais_ProtocolosVacinais_ProtocoloVacina~",
                        column: x => x.ProtocoloVacinalId,
                        principalTable: "ProtocolosVacinais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProtocoloVacinalDoses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProtocoloVacinalId = table.Column<Guid>(type: "uuid", nullable: false),
                    VacinaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    DiasAposInicio = table.Column<int>(type: "integer", nullable: false),
                    IntervaloReforcoDias = table.Column<int>(type: "integer", nullable: true),
                    Obrigatoria = table.Column<bool>(type: "boolean", nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    CreationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtocoloVacinalDoses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtocoloVacinalDoses_ProtocolosVacinais_ProtocoloVacinalId",
                        column: x => x.ProtocoloVacinalId,
                        principalTable: "ProtocolosVacinais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProtocoloVacinalDoses_Vacinas_VacinaId",
                        column: x => x.VacinaId,
                        principalTable: "Vacinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_AnimalProtocoloVacinalId",
                table: "AplicacoesVacina",
                column: "AnimalProtocoloVacinalId");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_ProtocoloVacinalDoseId",
                table: "AplicacoesVacina",
                column: "ProtocoloVacinalDoseId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalProtocolosVacinais_AnimalId_Ativo",
                table: "AnimalProtocolosVacinais",
                columns: new[] { "AnimalId", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_AnimalProtocolosVacinais_DataInicio",
                table: "AnimalProtocolosVacinais",
                column: "DataInicio");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalProtocolosVacinais_ProtocoloVacinalId",
                table: "AnimalProtocolosVacinais",
                column: "ProtocoloVacinalId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesAnimais_AnimalId_DataMovimentacao",
                table: "MovimentacoesAnimais",
                columns: new[] { "AnimalId", "DataMovimentacao" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesAnimais_LocalDestino",
                table: "MovimentacoesAnimais",
                column: "LocalDestino")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolosVacinais_EspecieId_Ativo",
                table: "ProtocolosVacinais",
                columns: new[] { "EspecieId", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolosVacinais_Nome",
                table: "ProtocolosVacinais",
                column: "Nome")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_ProtocoloVacinalDoses_ProtocoloVacinalId_Ordem",
                table: "ProtocoloVacinalDoses",
                columns: new[] { "ProtocoloVacinalId", "Ordem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProtocoloVacinalDoses_VacinaId",
                table: "ProtocoloVacinalDoses",
                column: "VacinaId");

            migrationBuilder.AddForeignKey(
                name: "FK_AplicacoesVacina_AnimalProtocolosVacinais_AnimalProtocoloVa~",
                table: "AplicacoesVacina",
                column: "AnimalProtocoloVacinalId",
                principalTable: "AnimalProtocolosVacinais",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AplicacoesVacina_ProtocoloVacinalDoses_ProtocoloVacinalDose~",
                table: "AplicacoesVacina",
                column: "ProtocoloVacinalDoseId",
                principalTable: "ProtocoloVacinalDoses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AplicacoesVacina_AnimalProtocolosVacinais_AnimalProtocoloVa~",
                table: "AplicacoesVacina");

            migrationBuilder.DropForeignKey(
                name: "FK_AplicacoesVacina_ProtocoloVacinalDoses_ProtocoloVacinalDose~",
                table: "AplicacoesVacina");

            migrationBuilder.DropTable(
                name: "AnimalProtocolosVacinais");

            migrationBuilder.DropTable(
                name: "MovimentacoesAnimais");

            migrationBuilder.DropTable(
                name: "ProtocoloVacinalDoses");

            migrationBuilder.DropTable(
                name: "ProtocolosVacinais");

            migrationBuilder.DropIndex(
                name: "IX_AplicacoesVacina_AnimalProtocoloVacinalId",
                table: "AplicacoesVacina");

            migrationBuilder.DropIndex(
                name: "IX_AplicacoesVacina_ProtocoloVacinalDoseId",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "AnimalProtocoloVacinalId",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "ProtocoloVacinalDoseId",
                table: "AplicacoesVacina");
        }
    }
}
