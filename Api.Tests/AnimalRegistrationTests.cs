using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Api.Tests;

public sealed class AnimalRegistrationTests(ClinicalFixture fixture) : IClassFixture<ClinicalFixture>
{
    [Theory]
    [InlineData("POST")]
    [InlineData("PATCH")]
    public async Task FutureBirthDateIsRejectedWithoutSavingOtherChanges(string method)
    {
        using var handler = fixture.CreateClient("manejador_animal");
        var original = await CreateAnimal(handler);
        var id = original.GetProperty("id").GetGuid();
        var patch = method == "PATCH";
        object body = patch ? new { name = "Nascimento futuro", dataNascimento = "2026-11-04" }
            : new
            {
                name = "Nascimento futuro",
                dataNascimento = "2026-11-04",
                identificadores = new[] { Identifier(Guid.NewGuid().ToString("N")) }
            };
        using var response = await ClinicalTestData.Mutate(handler, patch ? HttpMethod.Patch : HttpMethod.Post,
            patch ? $"/api/v1/animais/{id}" : "/api/v1/animais", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var persisted = await handler.GetFromJsonAsync<JsonElement>($"/api/v1/animais/{id}");
        Assert.Equal("Animal de cadastro", persisted.GetProperty("name").GetString());
        Assert.Equal("2026-11-03", persisted.GetProperty("dataNascimento").GetString());
        var matches = await handler.GetFromJsonAsync<JsonElement>("/api/v1/animais/search?page=1&search=Nascimento%20futuro");
        Assert.DoesNotContain(matches.EnumerateArray(), row => row.GetProperty("name").GetString() == "Nascimento futuro");
    }

    [Theory]
    [InlineData("PATCH", "vazia")]
    [InlineData("POST", "sem valor")]
    [InlineData("PATCH", "sem valor")]
    [InlineData("POST", "dois principais")]
    [InlineData("PATCH", "dois principais")]
    public async Task InvalidIdentifierSetIsRejectedWithoutErasingTheCurrentIdentification(string method, string issue)
    {
        using var handler = fixture.CreateClient("manejador_animal");
        var original = await CreateAnimal(handler);
        var id = original.GetProperty("id").GetGuid();
        var patch = method == "PATCH";
        var identifiers = issue switch
        {
            "vazia" => Array.Empty<object>(),
            "sem valor" => new[] { Identifier("  ") },
            _ => new[] { Identifier(Guid.NewGuid().ToString("N"), true), Identifier(Guid.NewGuid().ToString("N"), true) }
        };
        using var response = await ClinicalTestData.Mutate(handler, patch ? HttpMethod.Patch : HttpMethod.Post,
            patch ? $"/api/v1/animais/{id}" : "/api/v1/animais", new
            {
                dataNascimento = "2026-11-03",
                identificadores = identifiers
            });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var persisted = await handler.GetFromJsonAsync<JsonElement>($"/api/v1/animais/{id}");
        Assert.Equal(1, persisted.GetProperty("identificadores").GetArrayLength());
        Assert.Equal(original.GetProperty("identificadorPrincipal").GetProperty("valor").GetString(),
            persisted.GetProperty("identificadorPrincipal").GetProperty("valor").GetString());
    }

    [Fact]
    public async Task TodayIsValidAndEditPromotesFirstIdentifierWhenNoneIsMarked()
    {
        using var handler = fixture.CreateClient("manejador_animal");
        var original = await CreateAnimal(handler);
        var id = original.GetProperty("id").GetGuid();
        var value = original.GetProperty("identificadorPrincipal").GetProperty("valor").GetString()!;
        using var response = await ClinicalTestData.Mutate(handler, HttpMethod.Patch, $"/api/v1/animais/{id}", new
        {
            dataNascimento = "2026-11-03", dataNascimentoAproximada = true,
            identificadores = new[] { Identifier(value), Identifier(Guid.NewGuid().ToString("N")) }
        });
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        var result = await handler.GetFromJsonAsync<JsonElement>($"/api/v1/animais/{id}");
        Assert.True(result.GetProperty("dataNascimentoAproximada").GetBoolean());
        Assert.Equal(value, result.GetProperty("identificadorPrincipal").GetProperty("valor").GetString());
        Assert.Equal(original.GetProperty("identificadorPrincipal").GetProperty("id").GetGuid(),
            result.GetProperty("identificadorPrincipal").GetProperty("id").GetGuid());
        Assert.Single(result.GetProperty("identificadores").EnumerateArray(), row => row.GetProperty("ehPrincipal").GetBoolean());
    }

    [Fact]
    public async Task UpdatingLocationInRegistrationAppendsTransferAndExitToHistory()
    {
        using var handler = fixture.CreateClient("manejador_animal");
        var animal = await CreateAnimal(handler, "Pasto A");
        var id = animal.GetProperty("id").GetGuid();
        var route = $"/api/v1/animais/{id}";

        await AmbienteTests.CreateEnvironment(handler, "Pasto B");
        using var transfer = await ClinicalTestData.Mutate(handler, HttpMethod.Patch, route, new { loteOuPasto = "Pasto B" });
        Assert.True(transfer.IsSuccessStatusCode, await transfer.Content.ReadAsStringAsync());
        using var exit = await ClinicalTestData.Mutate(handler, HttpMethod.Patch, route, new { loteOuPasto = (string?)null });
        Assert.True(exit.IsSuccessStatusCode, await exit.Content.ReadAsStringAsync());

        var history = await handler.GetFromJsonAsync<JsonElement>($"{route}/movimentacoes");
        Assert.Equal(3, history.GetArrayLength());
        var movement = Assert.Single(history.EnumerateArray(), row => row.GetProperty("tipo").GetString() == "Transferencia");
        Assert.Equal("Pasto A", movement.GetProperty("localOrigem").GetString());
        Assert.Equal("Pasto B", movement.GetProperty("localDestino").GetString());
        var last = Assert.Single(history.EnumerateArray(), row => row.GetProperty("tipo").GetString() == "Saida");
        Assert.Equal("Pasto B", last.GetProperty("localOrigem").GetString());
        var persisted = await handler.GetFromJsonAsync<JsonElement>(route);
        Assert.Equal(JsonValueKind.Null, persisted.GetProperty("loteOuPasto").ValueKind);
        Assert.True(persisted.GetProperty("ativo").GetBoolean());
    }

    private static object Identifier(string value, bool principal = false) =>
        new { tipo = "BrincoVisual", valor = value, ehPrincipal = principal };

    private static async Task<JsonElement> CreateAnimal(HttpClient client, string? location = null)
    {
        if (location is not null) await AmbienteTests.CreateEnvironment(client, location);
        using var response = await ClinicalTestData.Mutate(client, HttpMethod.Post, "/api/v1/animais", new
        {
            name = "Animal de cadastro", dataNascimento = "2026-11-03", loteOuPasto = location,
            identificadores = new[] { Identifier(Guid.NewGuid().ToString("N")) }
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
}
