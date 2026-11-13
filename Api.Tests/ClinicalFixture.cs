extern alias VetApi;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VetControl.Testing;
using Xunit;
using ApiContext = VetApi::Api.Database.ApiContext;

namespace Api.Tests;

public sealed class ClinicalFixture : IAsyncLifetime
{
    public PostgresTestDatabase Database { get; } = new("api");
    public ClinicalApiFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        try
        {
            await Database.CreateAsync();
            Factory = new AuthorizedFactory(Database);
            using var scope = Factory.Services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<ApiContext>().Database.MigrateAsync();
        }
        catch { await DisposeAsync(); throw; }
    }

    public async Task DisposeAsync()
    {
        try { if (Factory is not null) await Factory.DisposeAsync(); }
        finally { await Database.DisposeAsync(); }
    }

    public HttpClient CreateClient(string? role = null)
    {
        var client = Factory.CreateClient();
        if (role is not null) client.DefaultRequestHeaders.Add("X-Test-Role", role);
        return client;
    }

    private sealed class AuthorizedFactory(PostgresTestDatabase database) : ClinicalApiFactory(database)
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(new FixedTimeProvider(new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.Zero)));
                services.AddAuthentication(options =>
                {
                options.DefaultAuthenticateScheme = TestAuthentication.SchemeName;
                options.DefaultChallengeScheme = TestAuthentication.SchemeName;
                options.DefaultForbidScheme = TestAuthentication.SchemeName;
                }).AddScheme<AuthenticationSchemeOptions, TestAuthentication>(TestAuthentication.SchemeName, _ => { });
            });
        }
    }
}

internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

// Compiled only in the test project. Production authentication and policies are not changed.
internal sealed class TestAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "ClinicalTests";
    public static readonly Guid UserId = Guid.Parse("b2e58635-d8a9-4851-a579-dc3104513d07");

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var role = Request.Headers["X-Test-Role"].ToString();
        if (string.IsNullOrWhiteSpace(role)) return Task.FromResult(AuthenticateResult.NoResult());
        var claims = new List<Claim>
        {
            new Claim("sub", UserId.ToString()), new Claim("name", Request.Headers["X-Test-Name"].FirstOrDefault() ?? "Veterinária de teste"),
            new Claim("role", role), new Claim("crmv", "12345"), new Claim("crmv_uf", "AM")
        };
        if (Request.Headers.ContainsKey("X-Test-No-License")) claims.RemoveAll(c => c.Type is "crmv" or "crmv_uf");
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName, "name", "role"));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}
