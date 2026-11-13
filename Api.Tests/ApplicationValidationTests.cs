extern alias VetApi;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ApiContext = VetApi::Api.Database.ApiContext;

namespace Api.Tests;

public sealed class ApplicationValidationTests(ClinicalFixture fixture) : IClassFixture<ClinicalFixture>
{
    [Theory]
    [InlineData("validade")]
    [InlineData("fabricacao")]
    [InlineData("proxima")]
    [InlineData("dose")]
    [InlineData("animal-inativo")]
    [InlineData("vacina-inativa")]
    [InlineData("especie")]
    public async Task SingleAndBatchRejectTheSameInvalidDataWithoutPersistingAnything(string scenario)
    {
        using var client = fixture.CreateClient("aplicador");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        if (scenario == "animal-inativo") (await db.Animals.SingleAsync(a => a.Id == ids.Animal)).Ativo = false;
        if (scenario == "vacina-inativa") (await db.Vacinas.SingleAsync(v => v.Id == ids.Vacina)).Ativo = false;
        if (scenario == "especie")
        {
            var species = new VetApi::Api.Models.Especie { Id = Guid.NewGuid(), Nome = "Restrição de espécie" };
            db.Add(species);
            (await db.Vacinas.SingleAsync(v => v.Id == ids.Vacina)).EspecieId = species.Id;
        }
        await db.SaveChangesAsync();
        var payload = new Dictionary<string, object?>
        {
            ["animalId"] = ids.Animal, ["vacinaId"] = ids.Vacina,
            ["numeroLote"] = "VALIDATION", ["dataAplicacao"] = "2026-01-10", ["doseMl"] = "2"
        };
        if (scenario == "validade") payload["dataValidade"] = "2026-01-09";
        if (scenario == "fabricacao") payload["dataFabricacao"] = "2026-01-11";
        if (scenario == "proxima") payload["dataProximaDose"] = "2026-01-09";
        if (scenario == "dose") payload["doseMl"] = "0";
        using var single = await ClinicalTestData.Mutate(client, HttpMethod.Post, "/api/v1/aplicacoes-vacina", payload);
        Assert.Equal(HttpStatusCode.BadRequest, single.StatusCode);
        payload.Remove("animalId"); payload["animais"] = new[] { new { animalId = ids.Animal } };
        using var batch = await ClinicalTestData.Mutate(client, HttpMethod.Post, "/api/v1/aplicacoes-vacina/lote", payload);
        Assert.Equal(HttpStatusCode.BadRequest, batch.StatusCode);
        Assert.False(await db.AplicacoesVacina.AnyAsync(a => a.AnimalId == ids.Animal));
    }

    [Fact]
    public async Task BatchIsAtomicAndRejectsDuplicateAnimals()
    {
        using var client = fixture.CreateClient("aplicador");
        var first = await ClinicalTestData.SeedAsync(fixture.Factory);
        var second = await ClinicalTestData.SeedAsync(fixture.Factory);
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        (await db.Animals.SingleAsync(a => a.Id == second.Animal)).Ativo = false;
        await db.SaveChangesAsync();
        using var invalid = await ClinicalTestData.Mutate(client, HttpMethod.Post, "/api/v1/aplicacoes-vacina/lote", new
        { vacinaId = first.Vacina, dataAplicacao = "2026-01-10", numeroLote = "ATOMIC", animais = new[] { new { animalId = first.Animal }, new { animalId = second.Animal } } });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        using var duplicate = await ClinicalTestData.Mutate(client, HttpMethod.Post, "/api/v1/aplicacoes-vacina/lote", new
        { vacinaId = first.Vacina, dataAplicacao = "2026-01-10", numeroLote = "DUPLICATE", animais = new[] { new { animalId = first.Animal }, new { animalId = first.Animal } } });
        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        Assert.False(await db.AplicacoesVacina.AnyAsync(a => a.AnimalId == first.Animal || a.AnimalId == second.Animal));
    }

    [Fact]
    public async Task PositiveDecimalDoseIsNormalized()
    {
        using var client = fixture.CreateClient("aplicador");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        using var response = await ClinicalTestData.Mutate(client, HttpMethod.Post, "/api/v1/aplicacoes-vacina", new
        { animalId = ids.Animal, vacinaId = ids.Vacina, dataAplicacao = "2026-01-10", numeroLote = "NORMALIZE", doseMl = "2,50" });
        response.EnsureSuccessStatusCode();
        Assert.Equal("2.5", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("doseMl").GetString());
    }
}
