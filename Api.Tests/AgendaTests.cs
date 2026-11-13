extern alias VetApi;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ApiContext = VetApi::Api.Database.ApiContext;

namespace Api.Tests;

public sealed class AgendaTests(ClinicalFixture fixture) : IClassFixture<ClinicalFixture>
{
    [Fact]
    public async Task PendingSignatureListShowsNewlyRecordedDosesOnFirstPageEvenWhenBackdated()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        foreach (var number in Enumerable.Range(1, 10))
        {
            using var older = await ClinicalTestData.Mutate(vet, HttpMethod.Post, "/api/v1/aplicacoes-vacina", new
            {
                animalId = ids.Animal, vacinaId = ids.Vacina, dataAplicacao = "2026-01-10", numeroLote = $"OLDER-{number}"
            });
            older.EnsureSuccessStatusCode();
        }
        using var recent = await ClinicalTestData.Mutate(vet, HttpMethod.Post, "/api/v1/aplicacoes-vacina", new
        {
            animalId = ids.Animal, vacinaId = ids.Vacina, dataAplicacao = "2026-11-03", numeroLote = "RECENT"
        });
        recent.EnsureSuccessStatusCode();
        using var backdated = await ClinicalTestData.Mutate(vet, HttpMethod.Post, "/api/v1/aplicacoes-vacina", new
        {
            animalId = ids.Animal, vacinaId = ids.Vacina, dataAplicacao = "2026-03-01", numeroLote = "RECENT-BACKDATED"
        });
        backdated.EnsureSuccessStatusCode();

        var rows = await vet.GetFromJsonAsync<JsonElement>($"/api/v1/aplicacoes-vacina/pendentes-assinatura?page=1&animalId={ids.Animal}");
        Assert.Equal(10, rows.GetArrayLength());
        Assert.Equal("RECENT-BACKDATED", rows[0].GetProperty("numeroLote").GetString());
        Assert.Equal("RECENT", rows[1].GetProperty("numeroLote").GetString());
    }

    [Fact]
    public async Task FirstDoseAppearsBeforeAnyVaccinationAndCancellationRestoresThePendingDose()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        var protocol = await ProtocolVersionTests.CreateProtocol(vet, ids.Vacina);
        var assignment = await ProtocolVersionTests.Assign(vet, ids.Animal, protocol.GetProperty("id").GetGuid());
        var query = $"animalId={ids.Animal}&status=Atrasada";
        var rows = await List(vet, query);
        Assert.Equal(1, rows.GetArrayLength());
        Assert.Equal("Protocolo", rows[0].GetProperty("origem").GetString());
        Assert.Equal(1, await Count(vet, query));
        using var application = await ClinicalTestData.Mutate(vet, HttpMethod.Post, "/api/v1/aplicacoes-vacina", new
        {
            animalId = ids.Animal, vacinaId = ids.Vacina, numeroLote = "AGENDA", dataAplicacao = "2026-01-10",
            animalProtocoloVacinalId = assignment.GetProperty("id").GetGuid(),
            protocoloVacinalDoseId = protocol.GetProperty("doses")[0].GetProperty("id").GetGuid()
        });
        application.EnsureSuccessStatusCode();
        var id = (await application.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        Assert.Equal(0, await Count(vet, query));
        Assert.Equal(1, await Count(vet, $"animalId={ids.Animal}&status=Concluida"));
        using var sign = await ClinicalTestData.Mutate(vet, HttpMethod.Post, $"/api/v1/aplicacoes-vacina/{id}/assinar", new { });
        sign.EnsureSuccessStatusCode();
        using var cancel = await ClinicalTestData.Mutate(vet, HttpMethod.Post, $"/api/v1/aplicacoes-vacina/{id}/cancelar", new { motivo = "Cancelamento conferido" });
        cancel.EnsureSuccessStatusCode();
        Assert.Equal(1, await Count(vet, query));
    }

    [Fact]
    public async Task StandaloneDosesAreNotDuplicatedAndTerminalDoseClosesOldReminders()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        foreach (var item in new[] { ("2026-01-10", "2026-02-10"), ("2026-02-10", "2026-03-10") })
        {
            using var response = await ClinicalTestData.Mutate(vet, HttpMethod.Post, "/api/v1/aplicacoes-vacina", new
            { animalId = ids.Animal, vacinaId = ids.Vacina, numeroLote = "AVULSA", dataAplicacao = item.Item1, dataProximaDose = item.Item2 });
            response.EnsureSuccessStatusCode();
        }
        var rows = await List(vet, $"animalId={ids.Animal}");
        Assert.Equal(1, rows.GetArrayLength());
        Assert.Equal("2026-03-10", rows[0].GetProperty("dataPrevista").GetString());
        using var final = await ClinicalTestData.Mutate(vet, HttpMethod.Post, "/api/v1/aplicacoes-vacina", new
        { animalId = ids.Animal, vacinaId = ids.Vacina, numeroLote = "FINAL", dataAplicacao = "2026-03-10" });
        final.EnsureSuccessStatusCode();
        Assert.Equal(0, await Count(vet, $"animalId={ids.Animal}"));
    }

    [Fact]
    public async Task TodayIsPendingAndInactiveAnimalsDisappearWithoutLosingHistory()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        using var application = await ClinicalTestData.Mutate(vet, HttpMethod.Post, "/api/v1/aplicacoes-vacina", new
        { animalId = ids.Animal, vacinaId = ids.Vacina, numeroLote = "HOJE", dataAplicacao = "2026-01-10", dataProximaDose = "2026-11-03" });
        application.EnsureSuccessStatusCode();
        Assert.Equal(1, await Count(vet, $"animalId={ids.Animal}&status=Pendente&dataPrevistaFrom=2026-11-03&dataPrevistaTo=2026-11-03"));
        Assert.Equal(0, await Count(vet, $"animalId={ids.Animal}&status=Atrasada"));
        using var deactivate = await ClinicalTestData.Mutate(vet, HttpMethod.Patch, $"/api/v1/animais/{ids.Animal}", new { ativo = false });
        deactivate.EnsureSuccessStatusCode();
        Assert.Equal(0, await Count(vet, $"animalId={ids.Animal}"));
        using var scope = fixture.Factory.Services.CreateScope();
        Assert.True(await scope.ServiceProvider.GetRequiredService<ApiContext>().AplicacoesVacina.AnyAsync(a => a.AnimalId == ids.Animal));
    }

    [Fact]
    public void ClinicDateUsesConfiguredTimeZoneAtUtcMidnight()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            { ["Clinic:TimeZone"] = "America/Manaus" }).Build();
        var clock = new VetApi::Api.Services.ClinicalCalendar(
            new FixedTimeProvider(new DateTimeOffset(2026, 1, 2, 1, 0, 0, TimeSpan.Zero)), config);
        Assert.Equal(new DateOnly(2026, 1, 1), clock.Today);
    }

    [Fact]
    public async Task LateFirstDoseCanBeRecordedWithoutInventingANextDoseInThePast()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        using var created = await ClinicalTestData.Mutate(vet, HttpMethod.Post, "/api/v1/protocolos-vacinais", new {
            nome = "Esquema atrasado", doses = new[] {
                new { vacinaId = ids.Vacina, ordem = 1, diasAposInicio = 0 },
                new { vacinaId = ids.Vacina, ordem = 2, diasAposInicio = 30 }
            }
        });
        created.EnsureSuccessStatusCode();
        var protocol = await created.Content.ReadFromJsonAsync<JsonElement>();
        var assignment = await ProtocolVersionTests.Assign(vet, ids.Animal, protocol.GetProperty("id").GetGuid());
        using var application = await ClinicalTestData.Mutate(vet, HttpMethod.Post, "/api/v1/aplicacoes-vacina", new {
            animalId = ids.Animal, vacinaId = ids.Vacina, dataAplicacao = "2026-11-03", numeroLote = "LATE-FIRST",
            animalProtocoloVacinalId = assignment.GetProperty("id").GetGuid(),
            protocoloVacinalDoseId = protocol.GetProperty("doses")[0].GetProperty("id").GetGuid()
        });
        Assert.True(application.IsSuccessStatusCode, await application.Content.ReadAsStringAsync());
        var recorded = await application.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Null, recorded.GetProperty("dataProximaDose").ValueKind);
        Assert.Equal(1, await Count(vet, $"animalId={ids.Animal}&status=Atrasada"));
        Assert.Equal(1, await Count(vet, $"animalId={ids.Animal}&status=Concluida"));
    }

    [Theory]
    [InlineData("atrasadas", "2026-11-02")]
    [InlineData("proximas", "2026-11-04")]
    public async Task CompatibilityRemindersDoNotCountCancelledOrReplacedApplications(string endpoint, string due)
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        using var create = await ClinicalTestData.Mutate(vet, HttpMethod.Post, "/api/v1/aplicacoes-vacina",
            new { animalId = ids.Animal, vacinaId = ids.Vacina, dataAplicacao = "2026-01-10", numeroLote = "COMPAT", dataProximaDose = due });
        create.EnsureSuccessStatusCode();
        var id = (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        using var sign = await ClinicalTestData.Mutate(vet, HttpMethod.Post, $"/api/v1/aplicacoes-vacina/{id}/assinar", new { });
        sign.EnsureSuccessStatusCode();
        using var fix = await ClinicalTestData.Mutate(vet, HttpMethod.Post, $"/api/v1/aplicacoes-vacina/{id}/retificar",
            new { motivo = "Correção de lote", alteracoes = new { numeroLote = "COMPAT-CORRECT" } });
        fix.EnsureSuccessStatusCode();
        var replacementId = (await fix.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var path = $"/api/v1/aplicacoes-vacina/{endpoint}";
        Assert.Equal(1, await vet.GetFromJsonAsync<int>($"{path}/count?animalId={ids.Animal}"));
        using var cancel = await ClinicalTestData.Mutate(vet, HttpMethod.Post, $"/api/v1/aplicacoes-vacina/{replacementId}/cancelar", new { motivo = "Cancelamento conferido" });
        cancel.EnsureSuccessStatusCode();
        Assert.Equal(0, await vet.GetFromJsonAsync<int>($"{path}/count?animalId={ids.Animal}"));
        Assert.Equal(0, (await vet.GetFromJsonAsync<JsonElement>($"{path}?animalId={ids.Animal}")).GetArrayLength());
    }

    private static async Task<JsonElement> List(HttpClient client, string query) =>
        await client.GetFromJsonAsync<JsonElement>("/api/v1/agenda-vacinal?" + query);
    private static async Task<int> Count(HttpClient client, string query) =>
        await client.GetFromJsonAsync<int>("/api/v1/agenda-vacinal/count?" + query);
}
