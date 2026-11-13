extern alias VetApi;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ApiContext = VetApi::Api.Database.ApiContext;
using Animal = VetApi::Api.Models.Animal;
using Vacina = VetApi::Api.Models.Vacina;

namespace Api.Tests;

public sealed class ClinicalFlowTests(ClinicalFixture fixture) : IClassFixture<ClinicalFixture>
{
    [Fact]
    public async Task MigrationsAreAppliedAndCanBeRepeatedWithoutLosingData()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        Assert.Equal(fixture.Database.ConnectionString, db.Database.GetConnectionString());
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        var animal = new Animal { Id = Guid.NewGuid(), Name = "Persistência", DataNascimento = new DateOnly(2020, 1, 1) };
        db.Animals.Add(animal);
        await db.SaveChangesAsync();
        await db.Database.MigrateAsync();
        Assert.True(await db.Animals.AsNoTracking().AnyAsync(x => x.Id == animal.Id));
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("aplicador", HttpStatusCode.Forbidden)]
    [InlineData("admin", HttpStatusCode.Forbidden)]
    public async Task SignaturePermissionIsEnforcedByTheApi(string? role, HttpStatusCode expected)
    {
        using var client = fixture.CreateClient(role);
        using var response = await client.PostAsJsonAsync($"/api/v1/aplicacoes-vacina/{Guid.NewGuid()}/assinar", new { });
        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task VaccinationCanBeRecordedSignedAndValidatedWithoutAuthentication()
    {
        using var client = fixture.CreateClient("medico_veterinario");
        var (animalId, vacinaId) = await SeedAsync();
        using var noCsrf = await client.PostAsJsonAsync("/api/v1/aplicacoes-vacina", new
        {
            animalId, vacinaId, dataAplicacao = "2026-01-10", numeroLote = "CLINICAL-001"
        });
        Assert.Equal(HttpStatusCode.BadRequest, noCsrf.StatusCode);

        using var created = await Mutate(client, HttpMethod.Post, "/api/v1/aplicacoes-vacina", new
        {
            animalId, vacinaId, dataAplicacao = "2026-01-10", numeroLote = "CLINICAL-001",
            doseMl = "2.0", laboratorioFabricante = "Laboratório de teste"
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        using var partialUpdate = await Mutate(client, HttpMethod.Patch, $"/api/v1/aplicacoes-vacina/{id}",
            new { observacoes = "Atualização parcial sem repetir a data" });
        Assert.Equal(HttpStatusCode.OK, partialUpdate.StatusCode);
        var updated = await partialUpdate.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("2026-01-10", updated.GetProperty("dataAplicacao").GetString());
        Assert.Equal("CLINICAL-001", updated.GetProperty("numeroLote").GetString());
        using var invalidDate = await Mutate(client, HttpMethod.Patch, $"/api/v1/aplicacoes-vacina/{id}",
            new { dataAplicacao = "9999-12-31" });
        Assert.Equal(HttpStatusCode.BadRequest, invalidDate.StatusCode);
        using var anonymous = fixture.CreateClient();
        var before = await anonymous.GetFromJsonAsync<JsonElement>($"/api/v1/validacao-vacinacao/{id}");
        Assert.True(before.GetProperty("encontrado").GetBoolean());
        Assert.False(before.GetProperty("assinada").GetBoolean());

        using var signature = await Mutate(client, HttpMethod.Post, $"/api/v1/aplicacoes-vacina/{id}/assinar", new { observacoes = "Conferido no teste" });
        Assert.Equal(HttpStatusCode.OK, signature.StatusCode);
        var validation = await anonymous.GetFromJsonAsync<JsonElement>($"/api/v1/validacao-vacinacao/{id}");
        Assert.True(validation.GetProperty("assinada").GetBoolean());
        Assert.True(validation.GetProperty("assinaturaIntegra").GetBoolean());
        Assert.Equal("Veterinária de teste", validation.GetProperty("assinante").GetString());

        using var patch = await Mutate(client, HttpMethod.Patch, $"/api/v1/aplicacoes-vacina/{id}", new { numeroLote = "CHANGED" });
        Assert.True(patch.StatusCode == HttpStatusCode.Conflict, await patch.Content.ReadAsStringAsync());
        using var repeated = await Mutate(client, HttpMethod.Post, $"/api/v1/aplicacoes-vacina/{id}/assinar", new { });
        Assert.Equal(HttpStatusCode.Conflict, repeated.StatusCode);
        using var deletion = await Mutate(client, HttpMethod.Delete, $"/api/v1/aplicacoes-vacina/{id}", new { });
        Assert.Equal(HttpStatusCode.Conflict, deletion.StatusCode);

        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        var audit = await db.AssinaturasAplicacoesVacina.SingleAsync(x => x.AplicacaoVacinaId == id);
        Assert.Equal(TestAuthentication.UserId, audit.AssinanteUserId);
        var application = await db.AplicacoesVacina.SingleAsync(x => x.Id == id);
        Assert.Equal(TestAuthentication.UserId, application.CreatedByUser);
        Assert.Equal("CLINICAL-001", application.NumeroLote);

        // Deliberately tamper only with this disposable database: verification must detect it.
        application.NumeroLote = "TAMPERED";
        await db.SaveChangesAsync();
        var tampered = await anonymous.GetFromJsonAsync<JsonElement>($"/api/v1/validacao-vacinacao/{id}");
        Assert.False(tampered.GetProperty("assinaturaIntegra").GetBoolean());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreationStillRequiresAnApplicationDate(bool explicitNull)
    {
        using var client = fixture.CreateClient("medico_veterinario");
        var (animalId, vacinaId) = await SeedAsync();
        var payload = new Dictionary<string, object?>
        {
            ["animalId"] = animalId, ["vacinaId"] = vacinaId, ["numeroLote"] = "REQUIRED-DATE"
        };
        if (explicitNull) payload["dataAplicacao"] = null;
        using var response = await Mutate(client, HttpMethod.Post, "/api/v1/aplicacoes-vacina", payload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        Assert.False(await db.AplicacoesVacina.AnyAsync(x => x.AnimalId == animalId));
    }

    private async Task<(Guid AnimalId, Guid VacinaId)> SeedAsync()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        var animal = new Animal { Id = Guid.NewGuid(), Name = "Animal de teste", DataNascimento = new DateOnly(2020, 1, 1) };
        var vacina = new Vacina { Id = Guid.NewGuid(), Name = "Vacina de teste" };
        db.AddRange(animal, vacina);
        await db.SaveChangesAsync();
        return (animal.Id, vacina.Id);
    }

    private static async Task<HttpResponseMessage> Mutate(HttpClient client, HttpMethod method, string url, object body)
    {
        var session = await client.GetFromJsonAsync<JsonElement>("/api/v1/auth/session");
        using var request = new HttpRequestMessage(method, url) { Content = JsonContent.Create(body) };
        request.Headers.Add("X-CSRF-TOKEN", session.GetProperty("csrfToken").GetString());
        return await client.SendAsync(request);
    }
}
