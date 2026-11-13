using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Api.Tests;

public sealed class ClinicalAccessTests(ClinicalFixture fixture) : IClassFixture<ClinicalFixture>
{
    [Fact]
    public async Task AccountWithoutClinicalRoleCannotReadClinicalData()
    {
        using var account = fixture.CreateClient("none");
        using var visitor = fixture.CreateClient();
        var ids = await ClinicalTestData.SeedAsync(fixture.Factory);
        foreach (var route in new[] { "/api/v1/agenda-vacinal", $"/api/v1/animais/{ids.Animal}/prontuario", "/api/v1/ambientes" })
            Assert.Equal(HttpStatusCode.Forbidden, (await account.GetAsync(route)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await visitor.GetAsync($"/api/v1/validacao-vacinacao/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task MalformedPublicCodeReturnsValidationMessageInPortuguese()
    {
        using var visitor = fixture.CreateClient();
        using var response = await visitor.GetAsync("/api/v1/validacao-vacinacao/invalido");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("valor informado não é válido", problem.GetProperty("errors").GetProperty("id")[0].GetString());
    }
}
