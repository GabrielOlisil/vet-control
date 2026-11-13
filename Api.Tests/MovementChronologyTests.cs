extern alias VetApi;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ApiContext = VetApi::Api.Database.ApiContext;

namespace Api.Tests;

public sealed class MovementChronologyTests(ClinicalFixture fixture) : IClassFixture<ClinicalFixture>
{
    [Fact]
    public async Task BackdatedMovementDoesNotOverwriteTheLatestLocation()
    {
        using var handler = fixture.CreateClient("manejador_animal");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        await Move(handler, ids.Animal, "2026-01-10", "Entrada", "Pasto A");
        await Move(handler, ids.Animal, "2026-03-10", "Transferencia", "Pasto C");
        await Move(handler, ids.Animal, "2026-02-10", "Transferencia", "Pasto B");
        var animal = await handler.GetFromJsonAsync<JsonElement>($"/api/v1/animais/{ids.Animal}");
        Assert.Equal("Pasto C", animal.GetProperty("loteOuPasto").GetString());
        var history = await handler.GetFromJsonAsync<JsonElement>($"/api/v1/animais/{ids.Animal}/movimentacoes");
        Assert.Equal(3, history.GetArrayLength());
        Assert.Equal("Pasto A", history.EnumerateArray().Single(m => m.GetProperty("localDestino").GetString() == "Pasto B")
            .GetProperty("localOrigem").GetString());
    }

    [Fact]
    public async Task SameDayEventsUseRecordingOrderAndExitDoesNotDeleteTheAnimal()
    {
        using var handler = fixture.CreateClient("manejador_animal");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        await Move(handler, ids.Animal, "2026-01-10", "Entrada", "A");
        await Move(handler, ids.Animal, "2026-01-10", "Transferencia", "B");
        var animal = await handler.GetFromJsonAsync<JsonElement>($"/api/v1/animais/{ids.Animal}");
        Assert.Equal("B", animal.GetProperty("loteOuPasto").GetString());
        await Move(handler, ids.Animal, "2026-01-10", "Saida", null);
        animal = await handler.GetFromJsonAsync<JsonElement>($"/api/v1/animais/{ids.Animal}");
        Assert.Equal(JsonValueKind.Null, animal.GetProperty("loteOuPasto").ValueKind);
        Assert.True(animal.GetProperty("ativo").GetBoolean());
    }

    [Fact]
    public async Task InvalidBatchRollsBackAndDuplicateOrEmptyIdsAreRejected()
    {
        using var handler = fixture.CreateClient("manejador_animal");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        await AmbienteTests.CreateEnvironment(handler, "Sem efeito");
        foreach (var animals in new[] { new[] { ids.Animal, Guid.NewGuid() }, new[] { ids.Animal, ids.Animal }, new[] { Guid.Empty } })
        {
            using var response = await ClinicalTestData.Mutate(handler, HttpMethod.Post, "/api/v1/movimentacoes-animais/lote", new
            { animalIds = animals, dataMovimentacao = "2026-01-10", tipo = "Entrada", localDestino = "Sem efeito" });
            Assert.True(response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound, await response.Content.ReadAsStringAsync());
        }
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        Assert.False(await db.MovimentacoesAnimais.AnyAsync(m => m.AnimalId == ids.Animal));
        Assert.Null((await db.Animals.SingleAsync(a => a.Id == ids.Animal)).LoteOuPasto);
    }

    [Fact]
    public async Task InactiveAnimalRequiresJustifiedAdjustmentAndCannotBypassItThroughAnimalPatch()
    {
        using var handler = fixture.CreateClient("manejador_animal");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        foreach (var name in new[] { "Mudança indevida", "Indevido", "Corrigido" })
            await AmbienteTests.CreateEnvironment(handler, name);
        using var deactivate = await ClinicalTestData.Mutate(handler, HttpMethod.Patch, $"/api/v1/animais/{ids.Animal}", new { ativo = false });
        deactivate.EnsureSuccessStatusCode();
        using var direct = await ClinicalTestData.Mutate(handler, HttpMethod.Patch, $"/api/v1/animais/{ids.Animal}", new { loteOuPasto = "Mudança indevida" });
        Assert.Equal(HttpStatusCode.BadRequest, direct.StatusCode);
        using var movement = await ClinicalTestData.Mutate(handler, HttpMethod.Post, $"/api/v1/animais/{ids.Animal}/movimentacoes",
            new { tipo = "Transferencia", dataMovimentacao = "2026-01-10", localDestino = "Indevido" });
        Assert.Equal(HttpStatusCode.BadRequest, movement.StatusCode);
        using var missingReason = await ClinicalTestData.Mutate(handler, HttpMethod.Post, $"/api/v1/animais/{ids.Animal}/movimentacoes",
            new { tipo = "Ajuste", dataMovimentacao = "2026-01-10", localDestino = "Corrigido" });
        Assert.Equal(HttpStatusCode.BadRequest, missingReason.StatusCode);
        using var valid = await ClinicalTestData.Mutate(handler, HttpMethod.Post, $"/api/v1/animais/{ids.Animal}/movimentacoes",
            new { tipo = "Ajuste", dataMovimentacao = "2026-01-10", localDestino = "Corrigido", motivo = "Correção histórica justificada" });
        Assert.Equal(HttpStatusCode.Created, valid.StatusCode);
    }

    private static async Task Move(HttpClient client, Guid animalId, string date, string type, string? destination)
    {
        if (destination is not null) await AmbienteTests.CreateEnvironment(client, destination);
        using var response = await ClinicalTestData.Mutate(client, HttpMethod.Post, $"/api/v1/animais/{animalId}/movimentacoes", new
        { dataMovimentacao = date, tipo = type, localDestino = destination });
        Assert.True(response.StatusCode == HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
    }
}
