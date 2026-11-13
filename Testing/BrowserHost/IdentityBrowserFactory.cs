extern alias IdentityApi;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using VetControl.Testing;
using AuthContext = IdentityApi::Auth.Database.AuthContext;

internal sealed class IdentityBrowserFactory(PostgresTestDatabase database) : WebApplicationFactory<IdentityApi::Program>
{
    private readonly TemporaryKeyDirectory keys = new();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OpenId:Issuer"] = "http://localhost:5189/", ["Frontend:Origin"] = "http://localhost:5187",
            ["OpenId:RedirectUri"] = "http://localhost:5188/signin-oidc",
            ["OpenId:PostLogoutRedirectUri"] = "http://localhost:5188/signout-callback-oidc"
        }));
        builder.ConfigureTestServices(services =>
        {
            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keys.Path));
            services.RemoveAll<AuthContext>(); services.RemoveAll<DbContextOptions<AuthContext>>();
            services.RemoveAll<Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration<AuthContext>>();
            services.AddDbContext<AuthContext>(options => options.UseNpgsql(database.ConnectionString).UseOpenIddict());
        });
    }
    public override async ValueTask DisposeAsync()
    {
        try { await base.DisposeAsync(); }
        finally { keys.Dispose(); }
    }
}
