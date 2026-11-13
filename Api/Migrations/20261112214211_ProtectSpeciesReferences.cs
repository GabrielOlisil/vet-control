using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ProtectSpeciesReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProtocolosVacinais_Especies_EspecieId",
                table: "ProtocolosVacinais");

            migrationBuilder.DropForeignKey(
                name: "FK_Racas_Especies_EspecieId",
                table: "Racas");

            migrationBuilder.DropForeignKey(
                name: "FK_Vacinas_Especies_EspecieId",
                table: "Vacinas");

            migrationBuilder.AddForeignKey(
                name: "FK_ProtocolosVacinais_Especies_EspecieId",
                table: "ProtocolosVacinais",
                column: "EspecieId",
                principalTable: "Especies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Racas_Especies_EspecieId",
                table: "Racas",
                column: "EspecieId",
                principalTable: "Especies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vacinas_Especies_EspecieId",
                table: "Vacinas",
                column: "EspecieId",
                principalTable: "Especies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProtocolosVacinais_Especies_EspecieId",
                table: "ProtocolosVacinais");

            migrationBuilder.DropForeignKey(
                name: "FK_Racas_Especies_EspecieId",
                table: "Racas");

            migrationBuilder.DropForeignKey(
                name: "FK_Vacinas_Especies_EspecieId",
                table: "Vacinas");

            migrationBuilder.AddForeignKey(
                name: "FK_ProtocolosVacinais_Especies_EspecieId",
                table: "ProtocolosVacinais",
                column: "EspecieId",
                principalTable: "Especies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Racas_Especies_EspecieId",
                table: "Racas",
                column: "EspecieId",
                principalTable: "Especies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vacinas_Especies_EspecieId",
                table: "Vacinas",
                column: "EspecieId",
                principalTable: "Especies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
