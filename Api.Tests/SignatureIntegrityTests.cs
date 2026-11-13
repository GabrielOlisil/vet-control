extern alias VetApi;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Security.Claims;
using System.Data.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ApiContext = VetApi::Api.Database.ApiContext;

namespace Api.Tests;

public sealed class SignatureIntegrityTests(ClinicalFixture fixture) : IClassFixture<ClinicalFixture>
{
    [Fact]
    public async Task ApplicatorCanRecordWithoutLicenseButVeterinarianCannotSignWithoutIt()
    {
        using var applicator = fixture.CreateClient("aplicador");
        applicator.DefaultRequestHeaders.Add("X-Test-No-License", "true");
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        var id = await ClinicalTestData.CreateApplication(applicator, ids.Animal, ids.Vacina);
        using var vet = fixture.CreateClient("medico_veterinario");
        vet.DefaultRequestHeaders.Add("X-Test-No-License", "true");
        using var sign = await ClinicalTestData.Mutate(vet, HttpMethod.Post, $"/api/v1/aplicacoes-vacina/{id}/assinar", new { });
        Assert.Equal(HttpStatusCode.BadRequest, sign.StatusCode);
        using var scope = fixture.Factory.Services.CreateScope();
        Assert.False(await scope.ServiceProvider.GetRequiredService<ApiContext>().AssinaturasAplicacoesVacina.AnyAsync(a => a.AplicacaoVacinaId == id));
    }

    [Fact]
    public async Task SigningAStaleReadRejectsConcurrentEditInsteadOfPersistingAnInvalidHash()
    {
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        using var client = fixture.CreateClient("medico_veterinario");
        var id = await ClinicalTestData.CreateApplication(client, ids.Animal, ids.Vacina);
        var gate = new SignatureUpdateGate();
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", TestAuthentication.UserId.ToString()),
                new Claim("name", "Veterinário concorrente"), new Claim("crmv", "12345"), new Claim("crmv_uf", "AM") }, "test")) } };
        await using var db = new ApiContext(new DbContextOptionsBuilder<ApiContext>().UseNpgsql(fixture.Database.ConnectionString)
            .AddInterceptors(gate).Options, accessor);
        var service = new VetApi::Api.Services.AplicacoesVacina.AplicacaoVacinaService(db, accessor,
            fixture.Factory.Services.GetRequiredService<VetApi::Api.Services.ClinicalCalendar>());
        var signing = service.AssinarAsync(id, new VetApi::Api.DTOs.AplicacaoVacinaAssinaturaDto());
        await gate.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        try
        {
            using var edit = await ClinicalTestData.Mutate(client, HttpMethod.Patch, $"/api/v1/aplicacoes-vacina/{id}", new { numeroLote = "CONCURRENT-EDIT" });
            edit.EnsureSuccessStatusCode();
        }
        finally { gate.Release.TrySetResult(); }
        await Assert.ThrowsAsync<InvalidOperationException>(() => signing);
        Assert.False(await db.AssinaturasAplicacoesVacina.AnyAsync(a => a.AplicacaoVacinaId == id));
        var record = await client.GetFromJsonAsync<JsonElement>($"/api/v1/aplicacoes-vacina/{id}");
        Assert.Equal("CONCURRENT-EDIT", record.GetProperty("numeroLote").GetString());
        Assert.Equal("PendenteAssinatura", record.GetProperty("statusComprovante").GetString());
    }

    private sealed class SignatureUpdateGate : DbCommandInterceptor
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (command.CommandText.StartsWith("UPDATE") && command.CommandText.Contains("HashAssinatura"))
            { Started.TrySetResult(); await Release.Task.WaitAsync(TimeSpan.FromSeconds(15), cancellationToken); }
            return result;
        }
    }

    [Fact]
    public async Task SignerAssumesResponsibilityAndCatalogChangesDoNotRewriteTheSignedDocument()
    {
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        using var applicator = fixture.CreateClient("aplicador");
        applicator.DefaultRequestHeaders.Add("X-Test-Name", "Aplicador original");
        var applicationId = await ClinicalTestData.CreateApplication(applicator, ids.Animal, ids.Vacina);
        using var vet = fixture.CreateClient("medico_veterinario");
        var before = await vet.GetFromJsonAsync<JsonElement>($"/api/v1/aplicacoes-vacina/{applicationId}");
        Assert.Equal("Aplicador original", before.GetProperty("aplicador").GetString());
        Assert.Equal(JsonValueKind.Null, before.GetProperty("veterinarioResponsavel").ValueKind);
        using var signature = await ClinicalTestData.Mutate(vet, HttpMethod.Post, $"/api/v1/aplicacoes-vacina/{applicationId}/assinar", new { });
        Assert.Equal(HttpStatusCode.OK, signature.StatusCode);

        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        var animal = await db.Animals.Include(a => a.Identificadores).SingleAsync(a => a.Id == ids.Animal);
        animal.Name = "Animal renomeado"; animal.Ativo = false;
        animal.Identificadores[0].Valor = "IDENTIFICACAO-ALTERADA-" + Guid.NewGuid().ToString("N");
        var vaccine = await db.Vacinas.SingleAsync(v => v.Id == ids.Vacina);
        vaccine.Name = "Vacina renomeada"; vaccine.Ativo = false;
        await db.SaveChangesAsync();
        using var anonymous = fixture.CreateClient();
        var check = await anonymous.GetFromJsonAsync<JsonElement>($"/api/v1/validacao-vacinacao/{applicationId}");
        Assert.True(check.GetProperty("assinaturaIntegra").GetBoolean());
        Assert.True(check.GetProperty("dadosHistoricosPreservados").GetBoolean());
        Assert.Equal("Animal original", check.GetProperty("animalIdentificacao").GetString());
        Assert.Equal("Vacina original", check.GetProperty("vacinaNome").GetString());
        Assert.Equal("Veterinária de teste", check.GetProperty("veterinarioResponsavel").GetString());
        var receipt = await vet.GetFromJsonAsync<JsonElement>($"/api/v1/aplicacoes-vacina/animal/{ids.Animal}/comprovante-vacinas/dados");
        var record = receipt.GetProperty("aplicacoesVacina").EnumerateArray().Single(a => a.GetProperty("id").GetGuid() == applicationId);
        Assert.Equal("Animal original", record.GetProperty("animal").GetProperty("name").GetString());
        Assert.Equal("Vacina original", record.GetProperty("vacina").GetProperty("name").GetString());
    }

    [Fact]
    public async Task OnlyOneConcurrentSignatureIsPersisted()
    {
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        using var first = fixture.CreateClient("medico_veterinario");
        using var second = fixture.CreateClient("medico_veterinario");
        var id = await ClinicalTestData.CreateApplication(first, ids.Animal, ids.Vacina);
        var responses = await Task.WhenAll(
            ClinicalTestData.Mutate(first, HttpMethod.Post, $"/api/v1/aplicacoes-vacina/{id}/assinar", new { }),
            ClinicalTestData.Mutate(second, HttpMethod.Post, $"/api/v1/aplicacoes-vacina/{id}/assinar", new { }));
        try
        {
            Assert.Single(responses, r => r.StatusCode == HttpStatusCode.OK);
            Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Conflict);
        }
        finally { foreach (var response in responses) response.Dispose(); }
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        Assert.Equal(1, await db.AssinaturasAplicacoesVacina.CountAsync(a => a.AplicacaoVacinaId == id));
    }

    [Fact]
    public async Task AnUpdateLoadedBeforeSigningCannotOverwriteTheSignature()
    {
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        using var client = fixture.CreateClient("medico_veterinario");
        var id = await ClinicalTestData.CreateApplication(client, ids.Animal, ids.Vacina);
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        var stale = await db.AplicacoesVacina.SingleAsync(a => a.Id == id);
        using var signature = await ClinicalTestData.Mutate(client, HttpMethod.Post, $"/api/v1/aplicacoes-vacina/{id}/assinar", new { });
        Assert.Equal(HttpStatusCode.OK, signature.StatusCode);
        stale.NumeroLote = "STALE-WRITE";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => db.SaveChangesAsync());
        var check = await client.GetFromJsonAsync<JsonElement>($"/api/v1/aplicacoes-vacina/{id}/validar-assinatura");
        Assert.True(check.GetProperty("integra").GetBoolean());
    }
}
