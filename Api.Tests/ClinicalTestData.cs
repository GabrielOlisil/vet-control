extern alias VetApi;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using VetControl.Testing;
using ApiContext = VetApi::Api.Database.ApiContext;

namespace Api.Tests;

internal static class ClinicalTestData
{
    public static async Task<(Guid Animal, Guid Vacina)> SeedAsync(ClinicalApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        var animal = new VetApi::Api.Models.Animal
            { Id = Guid.NewGuid(), Name = "Animal original", DataNascimento = new DateOnly(2020, 1, 1) };
        animal.Identificadores.Add(new VetApi::Api.Models.IdentificadorAnimal
            { Id = Guid.NewGuid(), Valor = Guid.NewGuid().ToString("N"), IsPrincipal = true });
        var vacina = new VetApi::Api.Models.Vacina { Id = Guid.NewGuid(), Name = "Vacina original" };
        db.AddRange(animal, vacina);
        await db.SaveChangesAsync();
        return (animal.Id, vacina.Id);
    }

    public static async Task<HttpResponseMessage> Mutate(HttpClient client, HttpMethod method, string path, object body)
    {
        var session = await client.GetFromJsonAsync<JsonElement>("/api/v1/auth/session");
        using var request = new HttpRequestMessage(method, path) { Content = JsonContent.Create(body) };
        request.Headers.Add("X-CSRF-TOKEN", session.GetProperty("csrfToken").GetString());
        return await client.SendAsync(request);
    }

    public static async Task<Guid> CreateApplication(HttpClient client, Guid animalId, Guid vacinaId)
    {
        using var response = await Mutate(client, HttpMethod.Post, "/api/v1/aplicacoes-vacina", new
        { animalId, vacinaId, dataAplicacao = "2026-01-10", numeroLote = "SNAPSHOT-001", doseMl = "2" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }
}
