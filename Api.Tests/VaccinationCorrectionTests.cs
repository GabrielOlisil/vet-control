extern alias VetApi;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ApiContext = VetApi::Api.Database.ApiContext;
using State = VetApi::Api.Models.Enums.SituacaoRegistroVacina;

namespace Api.Tests;

public sealed class VaccinationCorrectionTests(ClinicalFixture fixture) : IClassFixture<ClinicalFixture>
{
    [Theory]
    [InlineData("admin", "cancelar")]
    [InlineData("aplicador", "cancelar")]
    [InlineData("estagiario_medvet", "cancelar")]
    [InlineData("manejador_animal", "cancelar")]
    [InlineData("admin", "retificar")]
    [InlineData("aplicador", "retificar")]
    [InlineData("estagiario_medvet", "retificar")]
    [InlineData("manejador_animal", "retificar")]
    public async Task CorrectionRequiresVeterinarianRole(string role, string operation)
    {
        using var client = fixture.CreateClient(role);
        using var response = await ClinicalTestData.Mutate(client, HttpMethod.Post,
            $"/api/v1/aplicacoes-vacina/{Guid.NewGuid()}/{operation}", new { motivo = "Correção de teste", alteracoes = new { } });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CancellationPreservesSignatureAndPubliclyReportsItIsNotCurrent()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var id = await SignedRecord(vet);
        using var response = await ClinicalTestData.Mutate(vet, HttpMethod.Post,
            $"/api/v1/aplicacoes-vacina/{id}/cancelar", new { motivo = "Aplicação lançada por engano" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var anonymous = fixture.CreateClient();
        var result = await anonymous.GetFromJsonAsync<JsonElement>($"/api/v1/validacao-vacinacao/{id}");
        Assert.Equal("Cancelado", result.GetProperty("situacaoRegistro").GetString());
        Assert.False(result.GetProperty("registroVigente").GetBoolean());
        Assert.True(result.GetProperty("assinaturaIntegra").GetBoolean());
        using var repeated = await ClinicalTestData.Mutate(vet, HttpMethod.Post,
            $"/api/v1/aplicacoes-vacina/{id}/cancelar", new { motivo = "Tentativa repetida" });
        Assert.Equal(HttpStatusCode.Conflict, repeated.StatusCode);
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        Assert.Equal(2, await db.AssinaturasAplicacoesVacina.CountAsync(a => a.AplicacaoVacinaId == id));
    }

    [Fact]
    public async Task RectificationSignsReplacementWithoutDuplicatingThePhysicalDose()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var id = await SignedRecord(vet);
        using var response = await ClinicalTestData.Mutate(vet, HttpMethod.Post,
            $"/api/v1/aplicacoes-vacina/{id}/retificar", new { motivo = "Correção do lote digitado", alteracoes = new { numeroLote = "LOTE-CORRETO" } });
        Assert.True(response.StatusCode == HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        var replacement = await response.Content.ReadFromJsonAsync<JsonElement>();
        var newId = replacement.GetProperty("id").GetGuid();
        Assert.NotEqual(id, newId);
        Assert.Equal("Assinado", replacement.GetProperty("statusComprovante").GetString());
        Assert.Equal(id, replacement.GetProperty("retificaAplicacaoVacinaId").GetGuid());
        using var anonymous = fixture.CreateClient();
        var old = await anonymous.GetFromJsonAsync<JsonElement>($"/api/v1/validacao-vacinacao/{id}");
        Assert.Equal("Retificado", old.GetProperty("situacaoRegistro").GetString());
        Assert.False(old.GetProperty("registroVigente").GetBoolean());
        Assert.Equal(newId, old.GetProperty("substituidaPorId").GetGuid());
        Assert.True(old.GetProperty("assinaturaIntegra").GetBoolean());
        var current = await anonymous.GetFromJsonAsync<JsonElement>($"/api/v1/validacao-vacinacao/{newId}");
        Assert.True(current.GetProperty("assinaturaIntegra").GetBoolean());
        Assert.True(current.GetProperty("registroVigente").GetBoolean());
        Assert.Equal("LOTE-CORRETO", current.GetProperty("numeroLote").GetString());
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        var animalId = await db.AplicacoesVacina.Where(a => a.Id == id).Select(a => a.AnimalId).SingleAsync();
        Assert.Equal(1, await db.AplicacoesVacina.CountAsync(a => a.AnimalId == animalId && a.SituacaoRegistro == State.Ativo));
        var vaccineId = await db.AplicacoesVacina.Where(a => a.Id == id).Select(a => a.VacinaId).SingleAsync();
        var vaccine = await vet.GetFromJsonAsync<JsonElement>($"/api/v1/vacinas/{vaccineId}");
        Assert.Equal(1, vaccine.GetProperty("totalAplicacoes").GetInt32());
    }

    [Fact]
    public async Task InvalidCorrectionRollsBackReplacementAndOriginalState()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var id = await SignedRecord(vet);
        using var missingReason = await ClinicalTestData.Mutate(vet, HttpMethod.Post,
            $"/api/v1/aplicacoes-vacina/{id}/cancelar", new { motivo = " " });
        Assert.Equal(HttpStatusCode.BadRequest, missingReason.StatusCode);
        using var invalid = await ClinicalTestData.Mutate(vet, HttpMethod.Post,
            $"/api/v1/aplicacoes-vacina/{id}/retificar", new { motivo = "Correção inválida", alteracoes = new { numeroLote = " " } });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        Assert.Equal(State.Ativo, (await db.AplicacoesVacina.SingleAsync(a => a.Id == id)).SituacaoRegistro);
        Assert.False(await db.AplicacoesVacina.AnyAsync(a => a.RetificaAplicacaoVacinaId == id));
        Assert.Equal(1, await db.AssinaturasAplicacoesVacina.CountAsync(a => a.AplicacaoVacinaId == id));
    }

    [Fact]
    public async Task HistoricalApplicationCanBeRectifiedAfterClosingItsProtocol()
    {
        using var vet = fixture.CreateClient("medico_veterinario");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        var protocol = await ProtocolVersionTests.CreateProtocol(vet, ids.Vacina);
        var assignment = await ProtocolVersionTests.Assign(vet, ids.Animal, protocol.GetProperty("id").GetGuid());
        var assignmentId = assignment.GetProperty("id").GetGuid();
        using var created = await ClinicalTestData.Mutate(vet, HttpMethod.Post, "/api/v1/aplicacoes-vacina", new {
            animalId = ids.Animal, vacinaId = ids.Vacina, dataAplicacao = "2026-01-10", numeroLote = "HISTORICO",
            animalProtocoloVacinalId = assignmentId, protocoloVacinalDoseId = protocol.GetProperty("doses")[0].GetProperty("id").GetGuid()
        });
        created.EnsureSuccessStatusCode();
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        using var signed = await ClinicalTestData.Mutate(vet, HttpMethod.Post, $"/api/v1/aplicacoes-vacina/{id}/assinar", new { });
        signed.EnsureSuccessStatusCode();
        using var close = await ClinicalTestData.Mutate(vet, HttpMethod.Patch,
            $"/api/v1/animais/{ids.Animal}/protocolos-vacinais/{assignmentId}", new { ativo = false });
        close.EnsureSuccessStatusCode();
        using var correction = await ClinicalTestData.Mutate(vet, HttpMethod.Post, $"/api/v1/aplicacoes-vacina/{id}/retificar",
            new { motivo = "Correção histórica do lote", alteracoes = new { numeroLote = "LOTE-HISTORICO-CORRETO" } });
        Assert.True(correction.IsSuccessStatusCode, await correction.Content.ReadAsStringAsync());
        var replacement = await correction.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(assignmentId, replacement.GetProperty("animalProtocoloVacinalId").GetGuid());
        Assert.Equal("LOTE-HISTORICO-CORRETO", replacement.GetProperty("numeroLote").GetString());
    }

    private async Task<Guid> SignedRecord(HttpClient vet)
    {
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        var id = await ClinicalTestData.CreateApplication(vet, ids.Animal, ids.Vacina);
        using var signature = await ClinicalTestData.Mutate(vet, HttpMethod.Post, $"/api/v1/aplicacoes-vacina/{id}/assinar", new { });
        signature.EnsureSuccessStatusCode();
        return id;
    }
}
