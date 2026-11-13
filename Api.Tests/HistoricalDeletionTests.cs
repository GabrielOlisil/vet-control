extern alias VetApi;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;
using ApiContext = VetApi::Api.Database.ApiContext;
using Animal = VetApi::Api.Models.Animal;
using Vacina = VetApi::Api.Models.Vacina;
using Aplicacao = VetApi::Api.Models.AplicacaoVacina;
using Movimentacao = VetApi::Api.Models.MovimentacaoAnimal;
using Protocolo = VetApi::Api.Models.ProtocoloVacinal;
using Vinculo = VetApi::Api.Models.AnimalProtocoloVacinal;

namespace Api.Tests;

public sealed class HistoricalDeletionTests(ClinicalFixture fixture) : IClassFixture<ClinicalFixture>
{
    [Theory]
    [InlineData("vacina")]
    [InlineData("movimentacao")]
    [InlineData("protocolo")]
    public async Task AnimalWithHistoryCannotBeDeletedByApiOrDatabaseCascade(string history)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        var animal = new Animal { Id = Guid.NewGuid(), Name = "Histórico preservado", DataNascimento = new DateOnly(2020, 1, 1) };
        db.Animals.Add(animal);
        if (history == "vacina")
        {
            var vacina = new Vacina { Id = Guid.NewGuid(), Name = "Vacina histórica" };
            db.Add(vacina);
            db.Add(new Aplicacao { Id = Guid.NewGuid(), Animal = animal, Vacina = vacina, NumeroLote = "HISTORY", DataAplicacao = new DateOnly(2026, 1, 1) });
        }
        else if (history == "movimentacao")
            db.Add(new Movimentacao { Id = Guid.NewGuid(), Animal = animal, DataMovimentacao = new DateOnly(2026, 1, 1), LocalDestino = "Pasto histórico" });
        else
        {
            var protocolo = new Protocolo { Id = Guid.NewGuid(), Nome = "Protocolo histórico" };
            db.Add(protocolo);
            db.Add(new Vinculo { Id = Guid.NewGuid(), Animal = animal, ProtocoloVacinal = protocolo, DataInicio = new DateOnly(2026, 1, 1) });
        }
        await db.SaveChangesAsync();
        using var response = await Delete($"/api/v1/animais/{animal.Id}");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var violation = await Assert.ThrowsAsync<PostgresException>(() =>
            db.Animals.Where(x => x.Id == animal.Id).ExecuteDeleteAsync());
        Assert.Equal(PostgresErrorCodes.RestrictViolation, violation.SqlState);
        Assert.True(await db.Animals.AnyAsync(x => x.Id == animal.Id));
    }

    [Fact]
    public async Task VaccineWithApplicationsCannotEraseHistory()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        var animal = new Animal { Id = Guid.NewGuid(), DataNascimento = new DateOnly(2020, 1, 1) };
        var vacina = new Vacina { Id = Guid.NewGuid(), Name = "Vacina utilizada" };
        var application = new Aplicacao { Id = Guid.NewGuid(), Animal = animal, Vacina = vacina, NumeroLote = "KEEP", DataAplicacao = new DateOnly(2026, 1, 1) };
        db.AddRange(animal, vacina, application);
        await db.SaveChangesAsync();
        using var response = await Delete($"/api/v1/vacinas/{vacina.Id}");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var violation = await Assert.ThrowsAsync<PostgresException>(() =>
            db.Vacinas.Where(x => x.Id == vacina.Id).ExecuteDeleteAsync());
        Assert.Equal(PostgresErrorCodes.RestrictViolation, violation.SqlState);
        Assert.True(await db.AplicacoesVacina.AnyAsync(x => x.Id == application.Id));
    }

    [Fact]
    public async Task UnusedAnimalAndVaccineCanStillBeDeleted()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        var animal = new Animal { Id = Guid.NewGuid(), DataNascimento = new DateOnly(2020, 1, 1) };
        var vacina = new Vacina { Id = Guid.NewGuid(), Name = "Sem uso" };
        db.AddRange(animal, vacina);
        await db.SaveChangesAsync();
        using var animalResponse = await Delete($"/api/v1/animais/{animal.Id}");
        using var vaccineResponse = await Delete($"/api/v1/vacinas/{vacina.Id}");
        Assert.Equal(HttpStatusCode.NoContent, animalResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, vaccineResponse.StatusCode);
    }

    private async Task<HttpResponseMessage> Delete(string url)
    {
        using var client = fixture.CreateClient("admin");
        var session = await client.GetFromJsonAsync<JsonElement>("/api/v1/auth/session");
        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Add("X-CSRF-TOKEN", session.GetProperty("csrfToken").GetString());
        return await client.SendAsync(request);
    }
}
