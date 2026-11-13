extern alias VetApi;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Api.Tests;

public sealed class AmbienteTests(ClinicalFixture fixture) : IClassFixture<ClinicalFixture>
{
    private const string Catalog = "/api/v1/ambientes";

    [Fact]
    public async Task CatalogRequiresMovementPermissionToCreateAndSearchesLiteralNamesInPages()
    {
        using var manager = fixture.CreateClient("manejador_animal");
        using var applicator = fixture.CreateClient("aplicador");
        var prefix = $"Piquete {Guid.NewGuid():N}";
        using var forbidden = await ClinicalTestData.Mutate(applicator, HttpMethod.Post, Catalog,
            new { nome = prefix, tipo = "Piquete" });
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        using var blank = await ClinicalTestData.Mutate(manager, HttpMethod.Post, Catalog,
            new { nome = "   ", tipo = "Piquete" });
        Assert.Equal(HttpStatusCode.BadRequest, blank.StatusCode);
        for (var i = 0; i < 11; i++)
            await CreateEnvironment(manager, $"  {prefix} {i:00}%  ");
        await CreateEnvironment(manager, $"{prefix} outro");

        using var duplicate = await ClinicalTestData.Mutate(manager, HttpMethod.Post, Catalog,
            new { nome = $"{prefix.ToUpperInvariant()} 00%", tipo = "Pasto" });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        var filter = Uri.EscapeDataString(prefix + " ");
        var first = await applicator.GetFromJsonAsync<JsonElement>($"{Catalog}?search={filter}&page=1");
        var second = await applicator.GetFromJsonAsync<JsonElement>($"{Catalog}?search={filter}&page=2");
        Assert.Equal(10, first.GetArrayLength());
        Assert.Equal(2, second.GetArrayLength());
        Assert.Equal($"{prefix} 00%", first[0].GetProperty("nome").GetString());
        var literal = await applicator.GetFromJsonAsync<JsonElement>($"{Catalog}/count?search={Uri.EscapeDataString(prefix + " 00%")}");
        Assert.Equal(1, literal.GetInt32());
    }

    [Fact]
    public async Task MovesKeepEnvironmentIdentityAndOccupancyThroughBatchBackdatingAndExit()
    {
        using var manager = fixture.CreateClient("manejador_animal");
        var seed = await ClinicalTestData.SeedAsync(fixture.Factory);
        var first = seed.Animal;
        var second = (await ClinicalTestData.SeedAsync(fixture.Factory)).Animal;
        var pasture = await CreateEnvironment(manager, $"Pasto {Guid.NewGuid():N}");
        var paddock = await CreateEnvironment(manager, $"Piquete {Guid.NewGuid():N}");
        var empty = await CreateEnvironment(manager, $"Vazio {Guid.NewGuid():N}");

        using var unknown = await ClinicalTestData.Mutate(manager, HttpMethod.Post, $"/api/v1/animais/{first}/movimentacoes",
            new { tipo = "Entrada", dataMovimentacao = "2026-01-01", localDestino = "Desconhecido" });
        Assert.Equal(HttpStatusCode.BadRequest, unknown.StatusCode);
        using var nonexistent = await ClinicalTestData.Mutate(manager, HttpMethod.Post, $"/api/v1/animais/{first}/movimentacoes",
            new { tipo = "Entrada", dataMovimentacao = "2026-01-01", ambienteDestinoId = Guid.NewGuid() });
        Assert.Equal(HttpStatusCode.BadRequest, nonexistent.StatusCode);

        using var batch = await ClinicalTestData.Mutate(manager, HttpMethod.Post, "/api/v1/movimentacoes-animais/lote",
            new { tipo = "Entrada", dataMovimentacao = "2026-01-03", animalIds = new[] { first, second }, ambienteDestinoId = pasture });
        Assert.Equal(HttpStatusCode.OK, batch.StatusCode);
        Assert.Equal(2, await CountAnimals(manager, pasture));

        using var incorrectOrigin = await ClinicalTestData.Mutate(manager, HttpMethod.Post, "/api/v1/movimentacoes-animais/lote",
            new { tipo = "Transferencia", dataMovimentacao = "2026-01-04", animalIds = new[] { first, second }, ambienteOrigemId = empty, ambienteDestinoId = paddock });
        Assert.Equal(HttpStatusCode.BadRequest, incorrectOrigin.StatusCode);
        Assert.Equal(2, await CountAnimals(manager, pasture));

        await Move(manager, second, "2026-01-04", "Transferencia", paddock);
        using var noChange = await ClinicalTestData.Mutate(manager, HttpMethod.Post, "/api/v1/movimentacoes-animais/lote",
            new { tipo = "Transferencia", dataMovimentacao = "2026-01-05", animalIds = new[] { first, second }, ambienteDestinoId = paddock });
        Assert.Equal(HttpStatusCode.BadRequest, noChange.StatusCode);
        var problem = await noChange.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("já está no ambiente de destino", problem.GetProperty("detail").GetString());
        Assert.Equal(1, await CountAnimals(manager, pasture));
        Assert.Equal(1, await CountAnimals(manager, paddock));

        var transfer = await Move(manager, first, "2026-01-05", "Transferencia", paddock);
        Assert.Equal(pasture, transfer.GetProperty("ambienteOrigemId").GetGuid());
        Assert.Equal(paddock, transfer.GetProperty("ambienteDestinoId").GetGuid());
        await Move(manager, first, "2026-01-02", "Entrada", empty);
        var animal = await manager.GetFromJsonAsync<JsonElement>($"/api/v1/animais/{first}");
        Assert.Equal(paddock, animal.GetProperty("ambienteId").GetGuid());
        var historyCount = await manager.GetFromJsonAsync<int>($"/api/v1/movimentacoes-animais/count?AnimalId={first}&AmbienteDestinoId={paddock}");
        Assert.Equal(1, historyCount);

        await Move(manager, second, "2026-01-06", "Saida", null);
        var occupancy = await manager.GetFromJsonAsync<JsonElement>($"{Catalog}/ocupacao");
        Assert.Equal(0, Occupancy(occupancy, pasture));
        Assert.Equal(1, Occupancy(occupancy, paddock));
        Assert.Equal(0, Occupancy(occupancy, empty));
        Assert.False(occupancy.EnumerateArray().First().TryGetProperty("animais", out _));
        var animalList = await manager.GetFromJsonAsync<JsonElement>($"/api/v1/animais?AmbienteId={paddock}");
        Assert.Equal(first, Assert.Single(animalList.EnumerateArray()).GetProperty("id").GetGuid());

        using var vet = fixture.CreateClient("medico_veterinario");
        using var protocolResponse = await ClinicalTestData.Mutate(vet, HttpMethod.Post, "/api/v1/protocolos-vacinais",
            new { nome = "Protocolo de ocupação", doses = new[] { new { vacinaId = seed.Vacina, ordem = 1, diasAposInicio = 0 } } });
        protocolResponse.EnsureSuccessStatusCode();
        var protocol = (await protocolResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        await ProtocolVersionTests.Assign(vet, first, protocol);
        var agendaQuery = $"AnimalId={first}&AmbienteId={paddock}";
        Assert.Equal(1, await vet.GetFromJsonAsync<int>($"/api/v1/agenda-vacinal/count?{agendaQuery}"));
        var agenda = await vet.GetFromJsonAsync<JsonElement>($"/api/v1/agenda-vacinal?{agendaQuery}");
        Assert.Equal(paddock, Assert.Single(agenda.EnumerateArray()).GetProperty("ambienteId").GetGuid());
        Assert.Equal(0, await vet.GetFromJsonAsync<int>($"/api/v1/agenda-vacinal/count?AnimalId={first}&AmbienteId={pasture}"));
    }

    [Fact]
    public async Task RegistrationAndPatchAcceptEnvironmentAndPreserveLocationOnUnrelatedEdit()
    {
        using var manager = fixture.CreateClient("manejador_animal");
        var environment = await CreateEnvironment(manager, $"Baia {Guid.NewGuid():N}");
        var registration = new
        {
            name = "Animal com ambiente", dataNascimento = "2020-01-01", ambienteId = environment,
            identificadores = new[] { new { tipo = "BrincoVisual", valor = Guid.NewGuid().ToString("N") } }
        };
        using var created = await ClinicalTestData.Mutate(manager, HttpMethod.Post, "/api/v1/animais", registration);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var animal = await created.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(environment, animal.GetProperty("ambienteId").GetGuid());
        var route = $"/api/v1/animais/{animal.GetProperty("id").GetGuid()}";

        using var patch = await ClinicalTestData.Mutate(manager, HttpMethod.Patch, route, new { name = "Renomeado" });
        Assert.Equal(HttpStatusCode.OK, patch.StatusCode);
        Assert.Equal(1, await CountAnimals(manager, environment));
        using var clear = await ClinicalTestData.Mutate(manager, HttpMethod.Patch, route, new { ambienteId = (Guid?)null });
        Assert.Equal(HttpStatusCode.OK, clear.StatusCode);
        Assert.Equal(0, await CountAnimals(manager, environment));
        var history = await manager.GetFromJsonAsync<JsonElement>($"{route}/movimentacoes");
        Assert.Equal("Saida", history[0].GetProperty("tipo").GetString());
        Assert.Equal(environment, history[0].GetProperty("ambienteOrigemId").GetGuid());
        Assert.Equal(environment, history[1].GetProperty("ambienteDestinoId").GetGuid());
    }

    internal static async Task<Guid> CreateEnvironment(HttpClient client, string name)
    {
        using var created = await ClinicalTestData.Mutate(client, HttpMethod.Post, Catalog, new { nome = name, tipo = "Piquete" });
        Assert.True(created.StatusCode == HttpStatusCode.Created, await created.Content.ReadAsStringAsync());
        return (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    private static async Task<JsonElement> Move(HttpClient client, Guid animal, string date, string type, Guid? destination)
    {
        using var result = await ClinicalTestData.Mutate(client, HttpMethod.Post, $"/api/v1/animais/{animal}/movimentacoes",
            new { tipo = type, dataMovimentacao = date, ambienteDestinoId = destination });
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        return await result.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static Task<int> CountAnimals(HttpClient client, Guid environment) =>
        client.GetFromJsonAsync<int>($"/api/v1/animais/count?AmbienteId={environment}");

    private static int Occupancy(JsonElement rows, Guid environment) => rows.EnumerateArray()
        .Single(row => row.GetProperty("id").GetGuid() == environment).GetProperty("quantidadeAnimais").GetInt32();
}
