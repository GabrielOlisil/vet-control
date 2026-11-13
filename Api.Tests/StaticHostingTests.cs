using System.Net;
using Microsoft.AspNetCore.Hosting;
using VetControl.Testing;
using Xunit;

namespace Api.Tests;

public sealed class StaticHostingTests
{
    [Fact]
    public async Task AnonymousBrowserCanLoadTheBuiltSpaWhileClinicalEndpointsStayProtected()
    {
        using var directory = new TemporaryKeyDirectory();
        var webRoot = Path.Combine(directory.Path, "wwwroot");
        Directory.CreateDirectory(Path.Combine(webRoot, "_app", "immutable"));
        const string index = "<!doctype html><html><body>Vet Control static SPA</body></html>";
        const string script = "console.log('Vet Control');";
        await File.WriteAllTextAsync(Path.Combine(webRoot, "index.html"), index);
        await File.WriteAllTextAsync(Path.Combine(webRoot, "_app", "immutable", "app.js"), script);

        // Anonymous hosting and authorization do not require opening a database connection.
        await using var database = new PostgresTestDatabase("api");
        await using var application = new ClinicalApiFactory(database);
        await using var factory = application.WithWebHostBuilder(builder => builder.UseWebRoot(webRoot));
        using var client = factory.CreateClient();

        foreach (var path in new[] { "/", "/login", "/auth/login", "/prontuario/demo" })
        {
            var response = await client.GetAsync(path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(index, await response.Content.ReadAsStringAsync());
            Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        }
        var asset = await client.GetAsync("/_app/immutable/app.js");
        Assert.Equal(HttpStatusCode.OK, asset.StatusCode);
        Assert.Equal(script, await asset.Content.ReadAsStringAsync());
        Assert.Contains("javascript", asset.Content.Headers.ContentType?.MediaType);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/animais")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/unknown")).StatusCode);
    }
}
