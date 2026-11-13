extern alias VetApi;
using System.Net;
using System.Net.Http.Json;
using VetControl.Testing;
using Xunit;

namespace Api.Tests;

public sealed class PublicEndpointTests
{
    [Fact]
    public async Task ValidationIsRateLimitedEvenForMalformedCodes()
    {
        await using var db = new PostgresTestDatabase("api");
        await using var factory = new ClinicalApiFactory(db);
        using var client = factory.CreateClient();
        for (var request = 0; request < 60; request++)
        {
            using var response = await client.GetAsync("/api/v1/validacao-vacinacao/invalid-guid");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        using var limited = await client.GetAsync("/api/v1/validacao-vacinacao/invalid-guid");
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.NotNull(limited.Headers.RetryAfter);
    }

    [Fact]
    public async Task LivenessIsAnonymousAndUnknownApiDoesNotServeTheSpa()
    {
        await using var db = new PostgresTestDatabase("api");
        await using var factory = new ClinicalApiFactory(db);
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/live")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/not-an-endpoint")).StatusCode);
    }
}
