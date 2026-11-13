extern alias VetApi;
extern alias IdentityApi;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using VetControl.Testing;
using ApiContext = VetApi::Api.Database.ApiContext;
using Animal = VetApi::Api.Models.Animal;
using Vacina = VetApi::Api.Models.Vacina;
using AplicacaoVacina = VetApi::Api.Models.AplicacaoVacina;

if (Environment.GetEnvironmentVariable("VET_E2E_HOST") != "1")
    throw new InvalidOperationException("This test-only host must be started by the browser test runner.");

await using var database = new PostgresTestDatabase("e2e");
await database.CreateAsync();
await using var identityDatabase = new PostgresTestDatabase("auth");
await identityDatabase.CreateAsync();
await using (var identityContext = new IdentityApi::Auth.Database.AuthContext(
    new DbContextOptionsBuilder<IdentityApi::Auth.Database.AuthContext>()
        .UseNpgsql(identityDatabase.ConnectionString).UseOpenIddict().Options))
    await identityContext.Database.MigrateAsync();
await using var identity = new IdentityBrowserFactory(identityDatabase);
identity.UseKestrel(5189);
using (var admin = identity.CreateClient())
{
    const string password = "Browser-Test1!";
    using var setup = await AuthMutation(admin, "/api/auth/setup", new
    {
        fullName = "Admin E2E", userName = "e2e-admin", email = "admin@e2e.test", password, confirmPassword = password
    });
    setup.EnsureSuccessStatusCode();
    foreach (var user in new[] { ("e2e-vet", "Veterinário E2E", "medico_veterinario"), ("e2e-applicator", "Aplicador E2E", "aplicador") })
    {
        using var created = await AuthMutation(admin, "/api/auth/admin/users", new
        {
            fullName = user.Item2, userName = user.Item1, email = user.Item1 + "@e2e.test", password,
            roles = new[] { user.Item3 }, crmv = user.Item3 == "medico_veterinario" ? "12345" : null,
            crmvUf = user.Item3 == "medico_veterinario" ? "AM" : null, mustChangePassword = false
        });
        created.EnsureSuccessStatusCode();
    }
}
await using var factory = new BrowserFactory(database);
factory.UseKestrel(5188);
using (var scope = factory.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
    await db.Database.MigrateAsync();
    var animal = new Animal { Id = Guid.NewGuid(), Name = "Mimosa E2E", DataNascimento = new DateOnly(2020, 1, 1) };
    var vacina = new Vacina { Id = Guid.NewGuid(), Name = "Vacina E2E" };
    db.AddRange(animal, vacina);
    var protocol = new VetApi::Api.Models.ProtocoloVacinal { Id = Guid.NewGuid(), Nome = "Protocolo E2E" };
    protocol.Doses.Add(new VetApi::Api.Models.ProtocoloVacinalDose
    { Id = Guid.NewGuid(), Vacina = vacina, Ordem = 1, DiasAposInicio = 0 });
    db.Add(protocol);
    db.Add(new VetApi::Api.Models.AnimalProtocoloVacinal
    { Id = Guid.NewGuid(), Animal = animal, ProtocoloVacinal = protocol, DataInicio = new DateOnly(2026, 1, 10) });
    db.AplicacoesVacina.Add(new AplicacaoVacina
    {
        Id = Guid.Parse("7c5a2979-8cbc-40d9-ab25-b3c4a4c032b8"),
        Animal = animal, Vacina = vacina, DataAplicacao = new DateOnly(2026, 1, 10),
        NumeroLote = "E2E-001", Aplicador = "Aplicador de teste"
    });
    await db.SaveChangesAsync();
}

Console.WriteLine("VET_E2E_READY");
// Closing the runner's pipe also shuts down normally and disposes the owned database.
await Console.In.ReadLineAsync();

static async Task<HttpResponseMessage> AuthMutation(HttpClient client, string url, object body)
{
    var session = await client.GetFromJsonAsync<JsonElement>("/api/auth/session");
    using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
    request.Headers.Add("X-CSRF-TOKEN", session.GetProperty("csrfToken").GetString());
    return await client.SendAsync(request);
}

internal sealed class BrowserFactory(PostgresTestDatabase database) : ClinicalApiFactory(database)
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Authentication:Authority"] = "http://localhost:5189/",
            ["Authentication:FrontendOrigin"] = "http://localhost:5187"
        }));
        builder.ConfigureTestServices(services => services.PostConfigure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.Authority = "http://localhost:5189/";
            options.MetadataAddress = options.Authority + ".well-known/openid-configuration";
            options.ConfigurationManager = new Microsoft.IdentityModel.Protocols.ConfigurationManager<Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration>(
                options.MetadataAddress, new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfigurationRetriever(),
                new Microsoft.IdentityModel.Protocols.HttpDocumentRetriever(options.Backchannel) { RequireHttps = false });
        }));
        builder.ConfigureTestServices(services => services.Configure<CorsOptions>(options =>
            options.AddPolicy("AllowWeb", policy => policy.WithOrigins("http://localhost:5187")
                .AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("X-Clinic-TimeZone").AllowCredentials())));
    }
}
