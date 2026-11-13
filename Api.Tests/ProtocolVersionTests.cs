extern alias VetApi;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;
using ApiContext = VetApi::Api.Database.ApiContext;
using ProtocolService = VetApi::Api.Services.ProtocolosVacinais.IProtocoloVacinalService;

namespace Api.Tests;

public sealed class ProtocolVersionTests(ClinicalFixture fixture) : IClassFixture<ClinicalFixture>
{
    [Fact]
    public async Task ConcurrentEditVersionsTheDefinitionWhenTheFirstAssignmentCommits()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        var protocol = await CreateProtocol(vet, ids.Vacina);
        var id = protocol.GetProperty("id").GetGuid();
        var originalDose = protocol.GetProperty("doses")[0].GetProperty("id").GetGuid();

        await using var assigning = fixture.Factory.Services.CreateAsyncScope();
        await using var editing = fixture.Factory.Services.CreateAsyncScope();
        var assignmentDb = assigning.ServiceProvider.GetRequiredService<ApiContext>();
        var editDb = editing.ServiceProvider.GetRequiredService<ApiContext>();
        await editDb.Database.OpenConnectionAsync();

        await using var transaction = await assignmentDb.Database.BeginTransactionAsync();
        var assignment = await assigning.ServiceProvider.GetRequiredService<ProtocolService>().AssignToAnimalAsync(ids.Animal,
            new VetApi::Api.DTOs.AnimalProtocoloVacinalCreateDto { ProtocoloVacinalId = id, DataInicio = new DateOnly(2026, 1, 10) });
        var edit = editing.ServiceProvider.GetRequiredService<ProtocolService>().PatchAsync(id,
            new VetApi::Api.DTOs.ProtocoloVacinalPatchDto
            {
                Doses = [new() { VacinaId = ids.Vacina, Ordem = 1, DiasAposInicio = 30 }]
            });
        try
        {
            await WaitForPostgresLockOrCompletion(edit, editDb);
        }
        finally
        {
            await transaction.CommitAsync();
        }

        var revised = Assert.IsType<VetApi::Api.Models.ProtocoloVacinal>(await edit);
        Assert.NotEqual(id, revised.Id);
        Assert.Equal(2, revised.Versao);
        var retained = await vet.GetFromJsonAsync<JsonElement>($"/api/v1/animais/{ids.Animal}/protocolos-vacinais/{assignment.Id}");
        Assert.Equal(id, retained.GetProperty("protocoloVacinalId").GetGuid());
        Assert.Equal(originalDose, retained.GetProperty("doses")[0].GetProperty("protocoloVacinalDoseId").GetGuid());
        Assert.Equal("2026-01-10", retained.GetProperty("doses")[0].GetProperty("dataPrevista").GetString());
    }

    [Fact]
    public async Task AssignmentWaitingForADefinitionChangeRejectsTheNewlyHistoricalVersion()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        var protocol = await CreateProtocol(vet, ids.Vacina);
        var id = protocol.GetProperty("id").GetGuid();
        await using var editing = fixture.Factory.Services.CreateAsyncScope();
        await using var assigning = fixture.Factory.Services.CreateAsyncScope();
        var editDb = editing.ServiceProvider.GetRequiredService<ApiContext>();
        var assignmentDb = assigning.ServiceProvider.GetRequiredService<ApiContext>();
        await assignmentDb.Database.OpenConnectionAsync();

        await using var transaction = await editDb.Database.BeginTransactionAsync();
        var current = await editDb.ProtocolosVacinais
            .FromSqlInterpolated($"SELECT *, xmin FROM \"ProtocolosVacinais\" WHERE \"Id\" = {id} FOR UPDATE").SingleAsync();
        current.VersaoAtual = false;
        await editDb.SaveChangesAsync();
        editDb.ProtocolosVacinais.Add(new VetApi::Api.Models.ProtocoloVacinal
        {
            Id = Guid.NewGuid(), FamiliaId = current.FamiliaId, Versao = 2, Nome = "Versão concorrente",
            Doses = [new() { Id = Guid.NewGuid(), VacinaId = ids.Vacina, Ordem = 1, DiasAposInicio = 30 }]
        });
        await editDb.SaveChangesAsync();

        var assignment = assigning.ServiceProvider.GetRequiredService<ProtocolService>().AssignToAnimalAsync(ids.Animal,
            new VetApi::Api.DTOs.AnimalProtocoloVacinalCreateDto { ProtocoloVacinalId = id, DataInicio = new DateOnly(2026, 1, 10) });
        try
        {
            await WaitForPostgresLockOrCompletion(assignment, assignmentDb);
        }
        finally
        {
            await transaction.CommitAsync();
        }

        var rejected = await Assert.ThrowsAsync<ArgumentException>(() => assignment);
        Assert.Contains("versão atual", rejected.Message);
        Assert.Empty((await vet.GetFromJsonAsync<JsonElement>($"/api/v1/animais/{ids.Animal}/protocolos-vacinais")).EnumerateArray());
    }

    private async Task WaitForPostgresLockOrCompletion(Task operation, ApiContext context)
    {
        using var limit = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await using var observer = new NpgsqlConnection(fixture.Database.ConnectionString);
        await observer.OpenAsync(limit.Token);
        await using var waiting = new NpgsqlCommand("SELECT cardinality(pg_blocking_pids($1)) > 0", observer);
        waiting.Parameters.AddWithValue(((NpgsqlConnection)context.Database.GetDbConnection()).ProcessID);
        // Do not use a fixed delay to decide when the competing transaction can commit.
        while (!operation.IsCompleted && !((bool)(await waiting.ExecuteScalarAsync(limit.Token))!))
            await Task.Delay(20, limit.Token);
    }

    [Theory]
    [InlineData(false, int.MaxValue, null)]
    [InlineData(false, 0, int.MaxValue)]
    [InlineData(true, int.MaxValue, null)]
    [InlineData(true, 0, int.MaxValue)]
    public async Task DefinitionRejectsDelaysThatCannotProduceARepresentableCivilDate(bool editing, int offset, int? interval)
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        var protocol = editing ? await CreateProtocol(vet, ids.Vacina) : default;
        var route = editing ? $"/api/v1/protocolos-vacinais/{protocol.GetProperty("id").GetGuid()}" : "/api/v1/protocolos-vacinais";
        using var response = await ClinicalTestData.Mutate(vet, editing ? HttpMethod.Patch : HttpMethod.Post, route, new
        {
            nome = "Prazo fora do calendário",
            doses = new[] { new { vacinaId = ids.Vacina, ordem = 1, diasAposInicio = offset, intervaloReforcoDias = interval } }
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await vet.GetFromJsonAsync<int>("/api/v1/protocolos-vacinais/count?nome=Prazo%20fora%20do%20calend%C3%A1rio"));
        if (editing)
        {
            var current = await vet.GetFromJsonAsync<JsonElement>(route);
            Assert.Equal(0, current.GetProperty("doses")[0].GetProperty("diasAposInicio").GetInt32());
        }
    }

    [Fact]
    public async Task EditingDosesBeforeFirstAssignmentPersistsTheChangeInTheSameVersion()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var existing = await ClinicalTestData.SeedAsync(fixture.Factory);
        var replacement = await ClinicalTestData.SeedAsync(fixture.Factory);
        var protocol = await CreateProtocol(vet, existing.Vacina);
        var id = protocol.GetProperty("id").GetGuid();
        var route = $"/api/v1/protocolos-vacinais/{id}";
        using var edit = await ClinicalTestData.Mutate(vet, HttpMethod.Patch, route, new
        {
            doses = new[]
            {
                new { vacinaId = replacement.Vacina, ordem = 1, diasAposInicio = 7 },
                new { vacinaId = existing.Vacina, ordem = 2, diasAposInicio = 30 }
            }
        });
        Assert.True(edit.IsSuccessStatusCode, await edit.Content.ReadAsStringAsync());

        var persisted = await vet.GetFromJsonAsync<JsonElement>(route);
        Assert.Equal(id, persisted.GetProperty("id").GetGuid());
        Assert.Equal(1, persisted.GetProperty("versao").GetInt32());
        var doses = persisted.GetProperty("doses").EnumerateArray().ToArray();
        Assert.Equal(new[] { replacement.Vacina, existing.Vacina }, doses.Select(d => d.GetProperty("vacinaId").GetGuid()));
        Assert.Equal(new[] { 7, 30 }, doses.Select(d => d.GetProperty("diasAposInicio").GetInt32()));
        Assert.Single((await vet.GetFromJsonAsync<JsonElement>($"{route}/versoes")).EnumerateArray());
    }

    [Fact]
    public async Task EditingUsedProtocolCreatesVersionWithoutChangingOldAssignmentOrDose()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        var protocol = await CreateProtocol(vet, ids.Vacina);
        var id = protocol.GetProperty("id").GetGuid();
        var oldDose = protocol.GetProperty("doses")[0].GetProperty("id").GetGuid();
        var assignment = await Assign(vet, ids.Animal, id);
        var assignmentId = assignment.GetProperty("id").GetGuid();
        using var changed = await ClinicalTestData.Mutate(vet, HttpMethod.Patch, $"/api/v1/protocolos-vacinais/{id}",
            new { nome = "Protocolo revisado", doses = new[] { new { vacinaId = ids.Vacina, ordem = 1, diasAposInicio = 30 } } });
        Assert.True(changed.StatusCode == HttpStatusCode.OK, await changed.Content.ReadAsStringAsync());
        var next = await changed.Content.ReadFromJsonAsync<JsonElement>();
        var nextId = next.GetProperty("id").GetGuid();
        Assert.NotEqual(id, nextId);
        Assert.Equal(2, next.GetProperty("versao").GetInt32());
        var old = await vet.GetFromJsonAsync<JsonElement>($"/api/v1/protocolos-vacinais/{id}");
        Assert.False(old.GetProperty("versaoAtual").GetBoolean());
        Assert.Equal(oldDose, old.GetProperty("doses")[0].GetProperty("id").GetGuid());
        var oldAssignment = await vet.GetFromJsonAsync<JsonElement>($"/api/v1/animais/{ids.Animal}/protocolos-vacinais/{assignmentId}");
        Assert.Equal(id, oldAssignment.GetProperty("protocoloVacinalId").GetGuid());
        Assert.Equal(oldDose, oldAssignment.GetProperty("doses")[0].GetProperty("protocoloVacinalDoseId").GetGuid());
        var family = protocol.GetProperty("familiaId").GetGuid();
        var current = await vet.GetFromJsonAsync<JsonElement>($"/api/v1/protocolos-vacinais?familiaId={family}");
        Assert.Equal(1, current.GetArrayLength());
        Assert.Equal(nextId, current[0].GetProperty("id").GetGuid());
        var versions = await vet.GetFromJsonAsync<JsonElement>($"/api/v1/protocolos-vacinais/{id}/versoes");
        Assert.Equal(2, versions.GetArrayLength());

        using var editOld = await ClinicalTestData.Mutate(vet, HttpMethod.Patch, $"/api/v1/protocolos-vacinais/{id}", new { nome = "Proibido" });
        Assert.Equal(HttpStatusCode.Conflict, editOld.StatusCode);
        using var migration = await ClinicalTestData.Mutate(vet, HttpMethod.Post,
            $"/api/v1/animais/{ids.Animal}/protocolos-vacinais/{assignmentId}/migrar",
            new { protocoloDestinoId = nextId, dataInicio = "2026-02-01", motivo = "Novo esquema autorizado" });
        Assert.True(migration.StatusCode == HttpStatusCode.OK, await migration.Content.ReadAsStringAsync());
        var migrated = await migration.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(nextId, migrated.GetProperty("protocoloVacinalId").GetGuid());
        oldAssignment = await vet.GetFromJsonAsync<JsonElement>($"/api/v1/animais/{ids.Animal}/protocolos-vacinais/{assignmentId}");
        Assert.False(oldAssignment.GetProperty("ativo").GetBoolean());
        using var reopen = await ClinicalTestData.Mutate(vet, HttpMethod.Patch,
            $"/api/v1/animais/{ids.Animal}/protocolos-vacinais/{assignmentId}", new { ativo = true });
        Assert.Equal(HttpStatusCode.Conflict, reopen.StatusCode);
    }

    [Fact]
    public async Task EligibilityUsesAgeOnProtocolStartAndConcurrentAssignmentCannotDuplicate()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        using var second = fixture.CreateClient("medico_veterinario");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        using (var scope = fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
            (await db.Animals.SingleAsync(a => a.Id == ids.Animal)).DataNascimento = new DateOnly(2026, 1, 1);
            await db.SaveChangesAsync();
        }
        var protocol = await CreateProtocol(vet, ids.Vacina, 5, 15);
        var id = protocol.GetProperty("id").GetGuid();
        using var tooYoung = await ClinicalTestData.Mutate(vet, HttpMethod.Post,
            $"/api/v1/animais/{ids.Animal}/protocolos-vacinais", new { protocoloVacinalId = id, dataInicio = "2026-01-02" });
        Assert.Equal(HttpStatusCode.BadRequest, tooYoung.StatusCode);
        var responses = await Task.WhenAll(
            ClinicalTestData.Mutate(vet, HttpMethod.Post, $"/api/v1/animais/{ids.Animal}/protocolos-vacinais",
                new { protocoloVacinalId = id, dataInicio = "2026-01-10" }),
            ClinicalTestData.Mutate(second, HttpMethod.Post, $"/api/v1/animais/{ids.Animal}/protocolos-vacinais",
                new { protocoloVacinalId = id, dataInicio = "2026-01-10" }));
        try
        {
            Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
            Assert.Single(responses, r => r.StatusCode == HttpStatusCode.BadRequest);
        }
        finally { foreach (var response in responses) response.Dispose(); }
    }

    [Fact]
    public async Task PatchCanClearAgeLimitsAndCannotMoveStartOutsideEligibility()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        var protocol = await CreateProtocol(vet, ids.Vacina, 2000, 2500);
        var id = protocol.GetProperty("id").GetGuid();
        var assignment = await Assign(vet, ids.Animal, id);
        using var invalid = await ClinicalTestData.Mutate(vet, HttpMethod.Patch,
            $"/api/v1/animais/{ids.Animal}/protocolos-vacinais/{assignment.GetProperty("id").GetGuid()}",
            new { dataInicio = "2020-01-02" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        using var clear = await ClinicalTestData.Mutate(vet, HttpMethod.Patch,
            $"/api/v1/protocolos-vacinais/{id}", new { idadeMinimaDias = (int?)null, idadeMaximaDias = (int?)null });
        Assert.Equal(HttpStatusCode.OK, clear.StatusCode);
        var updated = await clear.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotEqual(id, updated.GetProperty("id").GetGuid());
        Assert.Equal(JsonValueKind.Null, updated.GetProperty("idadeMinimaDias").ValueKind);
        Assert.Equal(JsonValueKind.Null, updated.GetProperty("idadeMaximaDias").ValueKind);
        using var invalidRange = await ClinicalTestData.Mutate(vet, HttpMethod.Patch,
            $"/api/v1/protocolos-vacinais/{updated.GetProperty("id").GetGuid()}", new { idadeMinimaDias = -1 });
        Assert.Equal(HttpStatusCode.BadRequest, invalidRange.StatusCode);
    }

    internal static async Task<JsonElement> CreateProtocol(HttpClient vet, Guid vaccineId, int? minAge = null, int? maxAge = null)
    {
        using var response = await ClinicalTestData.Mutate(vet, HttpMethod.Post, "/api/v1/protocolos-vacinais",
            new { nome = "Protocolo original", idadeMinimaDias = minAge, idadeMaximaDias = maxAge,
                doses = new[] { new { vacinaId = vaccineId, ordem = 1, diasAposInicio = 0 } } });
        Assert.True(response.StatusCode == HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    internal static async Task<JsonElement> Assign(HttpClient vet, Guid animalId, Guid protocolId)
    {
        using var response = await ClinicalTestData.Mutate(vet, HttpMethod.Post, $"/api/v1/animais/{animalId}/protocolos-vacinais",
            new { protocoloVacinalId = protocolId, dataInicio = "2026-01-10" });
        Assert.True(response.StatusCode == HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
}
