extern alias VetApi;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using VetControl.Testing;
using static VetControl.Testing.BackupTool;
using Xunit;
using ApiContext = VetApi::Api.Database.ApiContext;

namespace Api.Tests;

public sealed class BackupRestoreTests(ClinicalFixture fixture) : IClassFixture<ClinicalFixture>
{
    [Fact]
    public async Task RealDumpRestorePreservesSignedRecordsAndRefusesOverwriteOrCorruption()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        var id = await ClinicalTestData.CreateApplication(vet, ids.Animal, ids.Vacina);
        using var sign = await ClinicalTestData.Mutate(vet, HttpMethod.Post, $"/api/v1/aplicacoes-vacina/{id}/assinar", new { });
        sign.EnsureSuccessStatusCode();
        await using var target = new PostgresTestDatabase("api");
        await target.CreateAsync();
        var root = FindRoot();
        var directory = Path.Combine(root, "artifacts", "tests", target.Name);
        Directory.CreateDirectory(directory);
        var file = Path.Combine(directory, "clinical.dump");
        var container = Environment.GetEnvironmentVariable("VET_TEST_PG_CONTAINER");
        if (string.IsNullOrEmpty(container))
        {
            var found = await Run("docker", ["ps", "-q", "--filter", "ancestor=postgres:18-alpine"], root);
            Assert.Equal(0, found.Exit);
            var containers = found.Output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            Assert.Single(containers);
            container = containers[0];
        }
        PostgresTestDatabase.ValidateName(fixture.Database.Name);
        PostgresTestDatabase.ValidateName(target.Name);
        async Task<(int Exit, string Output)> BackupTool(string operation, string database, params string[] extra) =>
            await Run("node", ["deployment/database-backup.mjs", operation, "--container", container!, "--database", database, "--file", file, ..extra], root);
        var dump = await BackupTool("backup", fixture.Database.Name);
        Assert.True(dump.Exit == 0, dump.Output);
        var missingConfirmation = await BackupTool("restore", target.Name);
        Assert.NotEqual(0, missingConfirmation.Exit);
        var restored = await BackupTool("restore", target.Name, "--expected-database", target.Name);
        Assert.True(restored.Exit == 0, restored.Output);
        await using (var factory = new ClinicalApiFactory(target))
        {
            using var client = factory.CreateClient();
            var result = await client.GetFromJsonAsync<JsonElement>($"/api/v1/validacao-vacinacao/{id}");
            Assert.True(result.GetProperty("assinaturaIntegra").GetBoolean());
            Assert.True(result.GetProperty("dadosHistoricosPreservados").GetBoolean());
            Assert.Equal("SNAPSHOT-001", result.GetProperty("numeroLote").GetString());
        }
        var overwrite = await BackupTool("restore", target.Name, "--expected-database", target.Name);
        Assert.NotEqual(0, overwrite.Exit);
        Assert.True(overwrite.Output.Contains("contém dados"), overwrite.Output);
        await File.AppendAllTextAsync(file, "corrupted");
        var corrupt = await BackupTool("restore", target.Name, "--expected-database", target.Name);
        Assert.NotEqual(0, corrupt.Exit);
        Assert.Contains("hash", corrupt.Output);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CurrentSeedExecutesOnMigratedDatabaseAndLegacySignaturesRemainVerifiable(bool upgradeExistingBaseline)
    {
        await using var database = new PostgresTestDatabase("api");
        await database.CreateAsync();
        await using var factory = new ClinicalApiFactory(database);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        if (upgradeExistingBaseline)
            await db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>().MigrateAsync("20261014152810_AddDistributedSessionsAndSignatureActor");
        else await db.Database.MigrateAsync();
        await using var connection = new NpgsqlConnection(database.ConnectionString);
        await connection.OpenAsync();
        var script = await File.ReadAllTextAsync(Path.Combine(FindRoot(), "seed.sql"));
        // Old installations have no family fields; all remaining data is the legacy fixture.
        if (upgradeExistingBaseline) script = script.Replace("UPDATE \"ProtocolosVacinais\" SET \"FamiliaId\" = \"Id\", \"Versao\" = 1, \"VersaoAtual\" = true;", "");
        await using var command = new NpgsqlCommand(script, connection) { CommandTimeout = 60 };
        await command.ExecuteNonQueryAsync();
        await db.Database.MigrateAsync();
        Assert.Equal(50, await db.Animals.CountAsync());
        Assert.Equal(20, await db.Vacinas.CountAsync());
        Assert.Equal(50, await db.Animals.CountAsync(a => a.AmbienteId != null));
        Assert.True(await db.MovimentacoesAnimais.AnyAsync(m => m.AmbienteDestinoId != null));
        var signed = await db.AplicacoesVacina.AsNoTracking().Where(a => a.HashAssinatura != null).Select(a => a.Id).ToListAsync();
        Assert.NotEmpty(signed);
        using var client = factory.CreateClient();
        foreach (var id in signed)
        {
            var result = await client.GetFromJsonAsync<JsonElement>($"/api/v1/validacao-vacinacao/{id}");
            Assert.True(result.GetProperty("assinaturaIntegra").GetBoolean());
            Assert.False(result.GetProperty("dadosHistoricosPreservados").GetBoolean());
        }
    }

}
