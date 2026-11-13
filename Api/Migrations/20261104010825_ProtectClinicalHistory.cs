using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ProtectClinicalHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnimalProtocolosVacinais_Animals_AnimalId",
                table: "AnimalProtocolosVacinais");

            migrationBuilder.DropForeignKey(
                name: "FK_AplicacoesVacina_AnimalProtocolosVacinais_AnimalProtocoloVa~",
                table: "AplicacoesVacina");

            migrationBuilder.DropForeignKey(
                name: "FK_AplicacoesVacina_Animals_AnimalId",
                table: "AplicacoesVacina");

            migrationBuilder.DropForeignKey(
                name: "FK_AplicacoesVacina_ProtocoloVacinalDoses_ProtocoloVacinalDose~",
                table: "AplicacoesVacina");

            migrationBuilder.DropForeignKey(
                name: "FK_AplicacoesVacina_Vacinas_VacinaId",
                table: "AplicacoesVacina");

            migrationBuilder.DropForeignKey(
                name: "FK_AssinaturasAplicacoesVacina_AplicacoesVacina_AplicacaoVacin~",
                table: "AssinaturasAplicacoesVacina");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacoesAnimais_Animals_AnimalId",
                table: "MovimentacoesAnimais");

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalProtocolosVacinais_Animals_AnimalId",
                table: "AnimalProtocolosVacinais",
                column: "AnimalId",
                principalTable: "Animals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AplicacoesVacina_AnimalProtocolosVacinais_AnimalProtocoloVa~",
                table: "AplicacoesVacina",
                column: "AnimalProtocoloVacinalId",
                principalTable: "AnimalProtocolosVacinais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AplicacoesVacina_Animals_AnimalId",
                table: "AplicacoesVacina",
                column: "AnimalId",
                principalTable: "Animals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AplicacoesVacina_ProtocoloVacinalDoses_ProtocoloVacinalDose~",
                table: "AplicacoesVacina",
                column: "ProtocoloVacinalDoseId",
                principalTable: "ProtocoloVacinalDoses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AplicacoesVacina_Vacinas_VacinaId",
                table: "AplicacoesVacina",
                column: "VacinaId",
                principalTable: "Vacinas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssinaturasAplicacoesVacina_AplicacoesVacina_AplicacaoVacin~",
                table: "AssinaturasAplicacoesVacina",
                column: "AplicacaoVacinaId",
                principalTable: "AplicacoesVacina",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacoesAnimais_Animals_AnimalId",
                table: "MovimentacoesAnimais",
                column: "AnimalId",
                principalTable: "Animals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnimalProtocolosVacinais_Animals_AnimalId",
                table: "AnimalProtocolosVacinais");

            migrationBuilder.DropForeignKey(
                name: "FK_AplicacoesVacina_AnimalProtocolosVacinais_AnimalProtocoloVa~",
                table: "AplicacoesVacina");

            migrationBuilder.DropForeignKey(
                name: "FK_AplicacoesVacina_Animals_AnimalId",
                table: "AplicacoesVacina");

            migrationBuilder.DropForeignKey(
                name: "FK_AplicacoesVacina_ProtocoloVacinalDoses_ProtocoloVacinalDose~",
                table: "AplicacoesVacina");

            migrationBuilder.DropForeignKey(
                name: "FK_AplicacoesVacina_Vacinas_VacinaId",
                table: "AplicacoesVacina");

            migrationBuilder.DropForeignKey(
                name: "FK_AssinaturasAplicacoesVacina_AplicacoesVacina_AplicacaoVacin~",
                table: "AssinaturasAplicacoesVacina");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacoesAnimais_Animals_AnimalId",
                table: "MovimentacoesAnimais");

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalProtocolosVacinais_Animals_AnimalId",
                table: "AnimalProtocolosVacinais",
                column: "AnimalId",
                principalTable: "Animals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AplicacoesVacina_AnimalProtocolosVacinais_AnimalProtocoloVa~",
                table: "AplicacoesVacina",
                column: "AnimalProtocoloVacinalId",
                principalTable: "AnimalProtocolosVacinais",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AplicacoesVacina_Animals_AnimalId",
                table: "AplicacoesVacina",
                column: "AnimalId",
                principalTable: "Animals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AplicacoesVacina_ProtocoloVacinalDoses_ProtocoloVacinalDose~",
                table: "AplicacoesVacina",
                column: "ProtocoloVacinalDoseId",
                principalTable: "ProtocoloVacinalDoses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AplicacoesVacina_Vacinas_VacinaId",
                table: "AplicacoesVacina",
                column: "VacinaId",
                principalTable: "Vacinas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssinaturasAplicacoesVacina_AplicacoesVacina_AplicacaoVacin~",
                table: "AssinaturasAplicacoesVacina",
                column: "AplicacaoVacinaId",
                principalTable: "AplicacoesVacina",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacoesAnimais_Animals_AnimalId",
                table: "MovimentacoesAnimais",
                column: "AnimalId",
                principalTable: "Animals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
