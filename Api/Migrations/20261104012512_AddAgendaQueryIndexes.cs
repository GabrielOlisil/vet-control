using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAgendaQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AplicacoesVacina_AnimalId",
                table: "AplicacoesVacina");

            migrationBuilder.DropIndex(
                name: "IX_AplicacoesVacina_AnimalProtocoloVacinalId",
                table: "AplicacoesVacina");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_AnimalId_VacinaId_SituacaoRegistro_DataApl~",
                table: "AplicacoesVacina",
                columns: new[] { "AnimalId", "VacinaId", "SituacaoRegistro", "DataAplicacao" });

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_AnimalProtocoloVacinalId_ProtocoloVacinalD~",
                table: "AplicacoesVacina",
                columns: new[] { "AnimalProtocoloVacinalId", "ProtocoloVacinalDoseId", "SituacaoRegistro", "DataAplicacao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AplicacoesVacina_AnimalId_VacinaId_SituacaoRegistro_DataApl~",
                table: "AplicacoesVacina");

            migrationBuilder.DropIndex(
                name: "IX_AplicacoesVacina_AnimalProtocoloVacinalId_ProtocoloVacinalD~",
                table: "AplicacoesVacina");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_AnimalId",
                table: "AplicacoesVacina",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_AnimalProtocoloVacinalId",
                table: "AplicacoesVacina",
                column: "AnimalProtocoloVacinalId");
        }
    }
}
