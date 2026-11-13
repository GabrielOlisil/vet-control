using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            migrationBuilder.CreateTable(
                name: "Especies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    NomeCientifico = table.Column<string>(type: "text", nullable: true),
                    PortePadrao = table.Column<int>(type: "integer", nullable: false),
                    IconeKey = table.Column<string>(type: "text", nullable: false),
                    CreationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUser = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUser = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especies", x => x.Id);
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
                    LastModificationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUser = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUser = table.Column<Guid>(type: "uuid", nullable: true)
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
                name: "Racas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EspecieId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    CreationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUser = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUser = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Racas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Racas_Especies_EspecieId",
                        column: x => x.EspecieId,
                        principalTable: "Especies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vacinas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EspecieId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    ObrigatorioOrgaoSanitario = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUser = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUser = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vacinas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vacinas_Especies_EspecieId",
                        column: x => x.EspecieId,
                        principalTable: "Especies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Animals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Sexo = table.Column<string>(type: "text", nullable: false),
                    RacaId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataNascimento = table.Column<DateOnly>(type: "date", nullable: false),
                    DataNascimentoAproximada = table.Column<bool>(type: "boolean", nullable: false),
                    OrigemAnimal = table.Column<int>(type: "integer", nullable: false),
                    LoteOuPasto = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUser = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUser = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Animals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Animals_Racas_RacaId",
                        column: x => x.RacaId,
                        principalTable: "Racas",
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
                    LastModificationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUser = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUser = table.Column<Guid>(type: "uuid", nullable: true)
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
                    LastModificationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUser = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUser = table.Column<Guid>(type: "uuid", nullable: true)
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
                name: "IdentificadorAnimal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnimalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<string>(type: "text", nullable: false),
                    IsPrincipal = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUser = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUser = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentificadorAnimal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentificadorAnimal_Animals_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesAnimais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GrupoId = table.Column<Guid>(type: "uuid", nullable: true),
                    AnimalId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataMovimentacao = table.Column<DateOnly>(type: "date", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    LocalOrigem = table.Column<string>(type: "text", nullable: true),
                    LocalDestino = table.Column<string>(type: "text", nullable: true),
                    Motivo = table.Column<string>(type: "text", nullable: true),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    CreationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUser = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUser = table.Column<Guid>(type: "uuid", nullable: true)
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
                name: "AplicacoesVacina",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VacinaId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnimalId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnimalProtocoloVacinalId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProtocoloVacinalDoseId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataAplicacao = table.Column<DateOnly>(type: "date", nullable: false),
                    DataProximaDose = table.Column<DateOnly>(type: "date", nullable: true),
                    StatusComprovante = table.Column<int>(type: "integer", nullable: false),
                    NumeroLote = table.Column<string>(type: "text", nullable: false),
                    DoseMl = table.Column<string>(type: "text", nullable: true),
                    DataFabricacao = table.Column<DateOnly>(type: "date", nullable: true),
                    DataValidade = table.Column<DateOnly>(type: "date", nullable: true),
                    LocalAplicacao = table.Column<string>(type: "text", nullable: true),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    VeterinarioResponsavel = table.Column<string>(type: "text", nullable: true),
                    VeterinarioCrmv = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VeterinarioCrmvUf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Aplicador = table.Column<string>(type: "text", nullable: true),
                    LaboratorioFabricante = table.Column<string>(type: "text", nullable: true),
                    AssinadaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Assinante = table.Column<string>(type: "text", nullable: true),
                    AssinanteCrmv = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AssinanteCrmvUf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    HashAssinatura = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    MetodoAssinatura = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModificationDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUser = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUser = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AplicacoesVacina", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AplicacoesVacina_AnimalProtocolosVacinais_AnimalProtocoloVa~",
                        column: x => x.AnimalProtocoloVacinalId,
                        principalTable: "AnimalProtocolosVacinais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AplicacoesVacina_Animals_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AplicacoesVacina_ProtocoloVacinalDoses_ProtocoloVacinalDose~",
                        column: x => x.ProtocoloVacinalDoseId,
                        principalTable: "ProtocoloVacinalDoses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AplicacoesVacina_Vacinas_VacinaId",
                        column: x => x.VacinaId,
                        principalTable: "Vacinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_Animals_Name",
                table: "Animals",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Animals_RacaId",
                table: "Animals",
                column: "RacaId");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_AnimalId",
                table: "AplicacoesVacina",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_AnimalProtocoloVacinalId",
                table: "AplicacoesVacina",
                column: "AnimalProtocoloVacinalId");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_DataAplicacao",
                table: "AplicacoesVacina",
                column: "DataAplicacao");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_DataProximaDose",
                table: "AplicacoesVacina",
                column: "DataProximaDose");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_HashAssinatura",
                table: "AplicacoesVacina",
                column: "HashAssinatura");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_ProtocoloVacinalDoseId",
                table: "AplicacoesVacina",
                column: "ProtocoloVacinalDoseId");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_StatusComprovante",
                table: "AplicacoesVacina",
                column: "StatusComprovante");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacoesVacina_VacinaId",
                table: "AplicacoesVacina",
                column: "VacinaId");

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturasAplicacoesVacina_AplicacaoVacinaId",
                table: "AssinaturasAplicacoesVacina",
                column: "AplicacaoVacinaId");

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturasAplicacoesVacina_DataHora",
                table: "AssinaturasAplicacoesVacina",
                column: "DataHora");

            migrationBuilder.CreateIndex(
                name: "IX_Especies_Nome",
                table: "Especies",
                column: "Nome")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Especies_NomeCientifico",
                table: "Especies",
                column: "NomeCientifico")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_IdentificadorAnimal_AnimalId",
                table: "IdentificadorAnimal",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentificadorAnimal_Valor",
                table: "IdentificadorAnimal",
                column: "Valor")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_IdentificadorAnimal_Valor_Tipo",
                table: "IdentificadorAnimal",
                columns: new[] { "Valor", "Tipo" },
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Racas_EspecieId",
                table: "Racas",
                column: "EspecieId");

            migrationBuilder.CreateIndex(
                name: "IX_Racas_Nome",
                table: "Racas",
                column: "Nome")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Vacinas_EspecieId",
                table: "Vacinas",
                column: "EspecieId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacinas_Name",
                table: "Vacinas",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssinaturasAplicacoesVacina");

            migrationBuilder.DropTable(
                name: "IdentificadorAnimal");

            migrationBuilder.DropTable(
                name: "MovimentacoesAnimais");

            migrationBuilder.DropTable(
                name: "AplicacoesVacina");

            migrationBuilder.DropTable(
                name: "AnimalProtocolosVacinais");

            migrationBuilder.DropTable(
                name: "ProtocoloVacinalDoses");

            migrationBuilder.DropTable(
                name: "Animals");

            migrationBuilder.DropTable(
                name: "ProtocolosVacinais");

            migrationBuilder.DropTable(
                name: "Vacinas");

            migrationBuilder.DropTable(
                name: "Racas");

            migrationBuilder.DropTable(
                name: "Especies");

            migrationBuilder.Sql("DROP EXTENSION pg_trgm;");

        }
    }
}
