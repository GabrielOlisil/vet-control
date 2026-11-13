using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Api.Tests;

public sealed class FunctionalRequirementsTests(ClinicalFixture fixture) : IClassFixture<ClinicalFixture>
{
    [Theory]
    [InlineData("raca")]
    [InlineData("vacina")]
    [InlineData("protocolo")]
    public async Task DeletingSpeciesCannotEraseOrLoosenExistingCatalogueReferences(string reference)
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var species = await Post(vet, "/api/v1/especies", new { nome = "Espécie referenciada" });
        var (path, id) = reference switch
        {
            "raca" => ("racas", await Post(vet, "/api/v1/racas", new { nome = "Raça com espécie", especieId = species })),
            "vacina" => ("vacinas", await Post(vet, "/api/v1/vacinas", new { name = "Vacina restrita", especieId = species })),
            _ => ("protocolos-vacinais", await CreateRestrictedProtocol(vet, species))
        };
        using var blocked = await ClinicalTestData.Mutate(vet, HttpMethod.Delete, $"/api/v1/especies/{species}", new { });
        Assert.Equal(HttpStatusCode.Conflict, blocked.StatusCode);
        var dependent = await vet.GetFromJsonAsync<JsonElement>($"/api/v1/{path}/{id}");
        Assert.Equal(species, dependent.GetProperty("especieId").GetGuid());
        Assert.Equal(species, Id(await vet.GetFromJsonAsync<JsonElement>($"/api/v1/especies/{species}")));

        var unused = await Post(vet, "/api/v1/especies", new { nome = "Espécie livre para exclusão" });
        using var removed = await ClinicalTestData.Mutate(vet, HttpMethod.Delete, $"/api/v1/especies/{unused}", new { });
        Assert.Equal(HttpStatusCode.NoContent, removed.StatusCode);
    }

    [Fact]
    public async Task CatalogueSearchUsesSpeciesAndActiveVaccineFilters()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var firstSpecies = await Post(vet, "/api/v1/especies", new { nome = "Espécie do catálogo A" });
        var secondSpecies = await Post(vet, "/api/v1/especies", new { nome = "Espécie do catálogo B" });
        var breed = await Post(vet, "/api/v1/racas", new { nome = "Raça do catálogo A", especieId = firstSpecies });
        await Post(vet, "/api/v1/racas", new { nome = "Raça do catálogo B", especieId = secondSpecies });
        var current = await Post(vet, "/api/v1/vacinas", new { name = "Vacina de catálogo A", especieId = firstSpecies });
        var retired = await Post(vet, "/api/v1/vacinas", new { name = "Vacina de catálogo B", especieId = firstSpecies });
        await Post(vet, "/api/v1/vacinas", new { name = "Vacina de catálogo C", especieId = secondSpecies });
        using var archive = await ClinicalTestData.Mutate(vet, HttpMethod.Patch, $"/api/v1/vacinas/{retired}", new { ativo = false });
        archive.EnsureSuccessStatusCode();

        Assert.Equal(breed, Id(Assert.Single((await vet.GetFromJsonAsync<JsonElement>(
            $"/api/v1/racas/search?page=1&especieId={firstSpecies}")).EnumerateArray())));
        Assert.Equal(current, Id(Assert.Single((await vet.GetFromJsonAsync<JsonElement>(
            $"/api/v1/vacinas/search?page=1&especieId={firstSpecies}&ativo=true")).EnumerateArray())));
        Assert.Equal(1, await vet.GetFromJsonAsync<int>($"/api/v1/vacinas/count?especieId={firstSpecies}&ativo=false"));
    }

    [Fact]
    public async Task ValidMovementBatchGroupsEventsAndUpdatesEachAnimalLocation()
    {
        using var handler = fixture.CreateClient("manejador_animal");
        var first = await ClinicalTestData.SeedAsync(fixture.Factory);
        var second = await ClinicalTestData.SeedAsync(fixture.Factory);
        await AmbienteTests.CreateEnvironment(handler, "Pasto em grupo");
        using var response = await ClinicalTestData.Mutate(handler, HttpMethod.Post, "/api/v1/movimentacoes-animais/lote", new
        {
            animalIds = new[] { first.Animal, second.Animal }, dataMovimentacao = "2026-11-03",
            tipo = "Entrada", localDestino = "Pasto em grupo"
        });
        response.EnsureSuccessStatusCode();
        var events = (await response.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray().ToArray();
        Assert.Equal(2, events.Length);
        var group = events[0].GetProperty("grupoId").GetGuid();
        Assert.NotEqual(Guid.Empty, group);
        Assert.All(events, row => Assert.Equal(group, row.GetProperty("grupoId").GetGuid()));
        foreach (var animalId in new[] { first.Animal, second.Animal })
        {
            var detail = await handler.GetFromJsonAsync<JsonElement>($"/api/v1/animais/{animalId}");
            Assert.Equal("Pasto em grupo", detail.GetProperty("loteOuPasto").GetString());
            var history = await handler.GetFromJsonAsync<JsonElement>($"/api/v1/animais/{animalId}/movimentacoes");
            Assert.Equal(group, Assert.Single(history.EnumerateArray()).GetProperty("grupoId").GetGuid());
        }
    }

    [Fact]
    public async Task ProtocolDefinitionRejectsRepeatedOrderAndInvalidIntervals()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        var invalid = new[] {
            new[] { new { vacinaId = ids.Vacina, ordem = 1, diasAposInicio = 0, intervaloReforcoDias = 30 },
                new { vacinaId = ids.Vacina, ordem = 1, diasAposInicio = 14, intervaloReforcoDias = 30 } },
            new[] { new { vacinaId = ids.Vacina, ordem = 1, diasAposInicio = 0, intervaloReforcoDias = 0 } },
            new[] { new { vacinaId = ids.Vacina, ordem = 1, diasAposInicio = -1, intervaloReforcoDias = 30 } }
        };
        foreach (var doses in invalid)
        {
            using var response = await ClinicalTestData.Mutate(vet, HttpMethod.Post, "/api/v1/protocolos-vacinais",
                new { nome = "Esquema inválido", doses });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        Assert.Equal(0, await vet.GetFromJsonAsync<int>("/api/v1/protocolos-vacinais/count?nome=Esquema%20inv%C3%A1lido"));
    }

    [Fact]
    public async Task ApplicationRequiresBothProtocolKeysAndTheCorrectAnimalAndVaccine()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        var other = await ClinicalTestData.SeedAsync(fixture.Factory);
        var protocol = await ProtocolVersionTests.CreateProtocol(vet, ids.Vacina);
        var assignment = await ProtocolVersionTests.Assign(vet, ids.Animal, Id(protocol));
        var dose = Id(protocol.GetProperty("doses")[0]);
        foreach (var (animal, vaccine, doseId) in new (Guid, Guid, Guid?)[]
            { (ids.Animal, ids.Vacina, null), (other.Animal, ids.Vacina, dose), (ids.Animal, other.Vacina, dose) })
        {
            using var rejected = await ClinicalTestData.Mutate(vet, HttpMethod.Post, "/api/v1/aplicacoes-vacina", new
            {
                animalId = animal, vacinaId = vaccine, dataAplicacao = "2026-11-03", numeroLote = "VINCULO",
                animalProtocoloVacinalId = Id(assignment), protocoloVacinalDoseId = doseId
            });
            Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        }
        Assert.Equal(0, await vet.GetFromJsonAsync<int>($"/api/v1/aplicacoes-vacina/count?animalId={ids.Animal}"));
        Assert.Equal(0, await vet.GetFromJsonAsync<int>($"/api/v1/aplicacoes-vacina/count?animalId={other.Animal}"));
    }

    [Fact]
    public async Task MandatoryAgendaFilterAndCountAgreeAcrossPages()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        var protocol = await Post(vet, "/api/v1/protocolos-vacinais", new
        {
            nome = "Calendário paginado",
            doses = Enumerable.Range(1, 12).Select(order => new
                { vacinaId = ids.Vacina, ordem = order, diasAposInicio = order - 1, obrigatoria = order < 12 })
        });
        await ProtocolVersionTests.Assign(vet, ids.Animal, protocol);
        var query = $"animalId={ids.Animal}&somenteObrigatorias=true&origem=Protocolo";
        Assert.Equal(11, await vet.GetFromJsonAsync<int>($"/api/v1/agenda-vacinal/count?{query}"));
        var first = await vet.GetFromJsonAsync<JsonElement>($"/api/v1/agenda-vacinal?page=1&{query}");
        var second = await vet.GetFromJsonAsync<JsonElement>($"/api/v1/agenda-vacinal?page=2&{query}");
        Assert.Equal(Enumerable.Range(1, 10), first.EnumerateArray().Select(row => row.GetProperty("ordem").GetInt32()));
        Assert.Equal(11, Assert.Single(second.EnumerateArray()).GetProperty("ordem").GetInt32());
    }

    [Fact]
    public async Task OnlySignedCurrentApplicationCanBeCancelledOrRetified()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        var application = await ClinicalTestData.CreateApplication(vet, ids.Animal, ids.Vacina);
        var route = $"/api/v1/aplicacoes-vacina/{application}";
        using var unsigned = await ClinicalTestData.Mutate(vet, HttpMethod.Post, $"{route}/cancelar", new { motivo = "Erro de digitação" });
        Assert.Equal(HttpStatusCode.Conflict, unsigned.StatusCode);
        using var sign = await ClinicalTestData.Mutate(vet, HttpMethod.Post, $"{route}/assinar", new { });
        sign.EnsureSuccessStatusCode();
        using var cancel = await ClinicalTestData.Mutate(vet, HttpMethod.Post, $"{route}/cancelar", new { motivo = "Erro de digitação" });
        cancel.EnsureSuccessStatusCode();
        using var correction = await ClinicalTestData.Mutate(vet, HttpMethod.Post, $"{route}/retificar", new
        {
            motivo = "Segunda alteração", alteracoes = new { numeroLote = "NAO-SALVAR" }
        });
        Assert.Equal(HttpStatusCode.Conflict, correction.StatusCode);
        var detail = await vet.GetFromJsonAsync<JsonElement>(route);
        Assert.Equal("Cancelado", detail.GetProperty("situacaoRegistro").GetString());
        Assert.Equal(JsonValueKind.Null, detail.GetProperty("substituidaPorId").ValueKind);
    }

    private static async Task<Guid> Post(HttpClient client, string path, object data)
    {
        using var response = await ClinicalTestData.Mutate(client, HttpMethod.Post, path, data);
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        return Id(await response.Content.ReadFromJsonAsync<JsonElement>());
    }

    private async Task<Guid> CreateRestrictedProtocol(HttpClient client, Guid species)
    {
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        return await Post(client, "/api/v1/protocolos-vacinais", new
        {
            nome = "Protocolo com espécie", especieId = species,
            doses = new[] { new { vacinaId = ids.Vacina, ordem = 1, diasAposInicio = 0 } }
        });
    }

    private static Guid Id(JsonElement row) => row.GetProperty("id").GetGuid();
}
