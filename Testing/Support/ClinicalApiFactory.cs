extern alias VetApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ApiContext = VetApi::Api.Database.ApiContext;

namespace VetControl.Testing;

/// <summary>Hosts the real API against an explicitly owned disposable database.</summary>
public class ClinicalApiFactory(PostgresTestDatabase database) : WebApplicationFactory<VetApi::Program>
{
    private readonly TemporaryKeyDirectory keys = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        PostgresTestDatabase.ValidateName(database.Name);
        builder.UseEnvironment("Development");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DataProtection:KeysPath"] = keys.Path,
            ["ConnectionStrings:ApiContext"] = database.ConnectionString
        }));
        builder.ConfigureTestServices(services =>
        {
            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keys.Path));
            services.RemoveAll<ApiContext>();
            services.RemoveAll<DbContextOptions<ApiContext>>();
            services.RemoveAll<Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration<ApiContext>>();
            services.AddDbContextPool<ApiContext>(options => options.UseNpgsql(database.ConnectionString));
        });
    }

    public override async ValueTask DisposeAsync()
    {
        try { await base.DisposeAsync(); }
        finally { keys.Dispose(); }
    }
}
