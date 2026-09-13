using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVeterinarioAplicadorLaboratorioToAplicacaoVacina : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VeterinarioResponsavel",
                table: "AplicacoesVacina",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Aplicador",
                table: "AplicacoesVacina",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LaboratorioFabricante",
                table: "AplicacoesVacina",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VeterinarioResponsavel",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "Aplicador",
                table: "AplicacoesVacina");

            migrationBuilder.DropColumn(
                name: "LaboratorioFabricante",
                table: "AplicacoesVacina");
        }
    }
}

