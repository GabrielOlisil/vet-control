using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Api.Database;

public sealed class ApiContextFactory : IDesignTimeDbContextFactory<ApiContext>
{
    public ApiContext CreateDbContext(string[] args)
    {
        var root = Directory.GetCurrentDirectory();
        if (Directory.Exists(Path.Combine(root, "Api"))) root = Path.Combine(root, "Api");
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var config = new ConfigurationBuilder().SetBasePath(root).AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true).AddEnvironmentVariables().Build();
        var connection = config.GetConnectionString("ApiContext")
            ?? throw new InvalidOperationException("Configure ConnectionStrings:ApiContext para executar migrations.");
        return new ApiContext(new DbContextOptionsBuilder<ApiContext>().UseNpgsql(connection).Options, new HttpContextAccessor());
    }
}
