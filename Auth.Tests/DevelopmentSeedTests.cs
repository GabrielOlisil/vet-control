using Auth.Database;
using Auth.Models;
using Auth.Security;
using Auth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using VetControl.Testing;
using Xunit;

namespace Auth.Tests;

public sealed class DevelopmentSeedTests : IAsyncLifetime
{
    private readonly PostgresTestDatabase database = new("auth");
    private ServiceProvider services = null!;
    private static readonly IConfiguration Enabled = Configuration("true");

    public async Task InitializeAsync()
    {
        try
        {
            await database.CreateAsync();
            services = BuildServices(database.ConnectionString);
            using var scope = services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<AuthContext>().Database.MigrateAsync();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            foreach (var (name, description) in RoleCatalog.Descriptions)
                Assert.True((await roles.CreateAsync(new ApplicationRole { Name = name, Description = description })).Succeeded);
        }
        catch { await DisposeAsync(); throw; }
    }

    public async Task DisposeAsync()
    {
        if (services is not null) await services.DisposeAsync();
        await database.DisposeAsync();
    }

    [Fact]
    public async Task CreatesLoginReadyAccountsWithDistinctRolesAndPreservesChangesOnRestart()
    {
        await DevelopmentSeed.Initialize(services, Enabled, new EnvironmentStub());
        using (var scope = services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = (await users.FindByNameAsync("admin"))!;
            var vet = (await users.FindByNameAsync("veterinario"))!;
            Assert.True(await users.CheckPasswordAsync(admin, "Demonstracao123!"));
            Assert.True(await users.CheckPasswordAsync(vet, "Demonstracao123!"));
            Assert.NotEqual("Demonstracao123!", admin.PasswordHash);
            Assert.Equal([RoleCatalog.Admin], await users.GetRolesAsync(admin));
            Assert.Equal([RoleCatalog.Veterinarian], await users.GetRolesAsync(vet));
            Assert.False(await users.IsInRoleAsync(admin, RoleCatalog.Veterinarian));
            Assert.False(await users.IsInRoleAsync(vet, RoleCatalog.Admin));
            Assert.Equal("DEMO-001", vet.Crmv);
            Assert.Equal("AM", vet.CrmvUf);
            Assert.True(admin.IsActive && vet.IsActive);
            Assert.False(admin.MustChangePassword || vet.MustChangePassword);
            Assert.True((await users.ChangePasswordAsync(admin, "Demonstracao123!", "Changed-Password1!")).Succeeded);
            vet.IsActive = false;
            Assert.True((await users.UpdateAsync(vet)).Succeeded);
        }

        await DevelopmentSeed.Initialize(services, Configuration("true", "Other-Password1!"), new EnvironmentStub());
        using var verify = services.CreateScope();
        var manager = verify.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.True(await manager.CheckPasswordAsync((await manager.FindByNameAsync("admin"))!, "Changed-Password1!"));
        Assert.False((await manager.FindByNameAsync("veterinario"))!.IsActive);
        var db = verify.ServiceProvider.GetRequiredService<AuthContext>();
        Assert.Equal(2, await db.Users.CountAsync());
        Assert.Equal(3, await db.AuditEvents.CountAsync());
        Assert.DoesNotContain("Demonstracao123!", string.Join(" ", await db.AuditEvents.Select(x => x.AfterJson).ToListAsync()));
    }

    [Fact]
    public async Task ConcurrentStartsCreateOnePairOfAccounts()
    {
        await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => DevelopmentSeed.Initialize(services, Enabled, new EnvironmentStub())));
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthContext>();
        Assert.Equal(2, await db.Users.CountAsync());
        Assert.Equal(1, await db.AuditEvents.CountAsync(x => x.Action == "development.seed.v1.completed"));
    }

    [Fact]
    public async Task RefusesOccupiedDatabaseWithoutChangingItsUser()
    {
        using (var scope = services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Assert.True((await users.CreateAsync(new ApplicationUser
            {
                Id = Guid.NewGuid(), UserName = "existing", FullName = "Existing user", Email = "existing@example.invalid"
            }, "Existing-Password1!")).Succeeded);
        }
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => DevelopmentSeed.Initialize(services, Enabled, new EnvironmentStub()));
        Assert.Contains("usuários existentes", error.Message);
        using var verify = services.CreateScope();
        var db = verify.ServiceProvider.GetRequiredService<AuthContext>();
        Assert.Equal("existing", (await db.Users.SingleAsync()).UserName);
        Assert.Empty(await db.AuditEvents.ToListAsync());
    }

    [Fact]
    public async Task InvalidPasswordRollsBackWithoutSeedMarker()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => DevelopmentSeed.Initialize(services, Configuration("true", "weak"), new EnvironmentStub()));
        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuthContext>();
            Assert.Empty(await db.Users.ToListAsync());
            Assert.Empty(await db.AuditEvents.ToListAsync());
        }
        await DevelopmentSeed.Initialize(services, Enabled, new EnvironmentStub());
        using var verify = services.CreateScope();
        Assert.Equal(2, await verify.ServiceProvider.GetRequiredService<AuthContext>().Users.CountAsync());
    }

    [Fact]
    public async Task FailureAfterFirstAccountRollsBackAllAccountsAndAudit()
    {
        using (var scope = services.CreateScope())
        {
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            Assert.True((await roles.DeleteAsync((await roles.FindByNameAsync(RoleCatalog.Veterinarian))!)).Succeeded);
        }
        await Assert.ThrowsAsync<InvalidOperationException>(() => DevelopmentSeed.Initialize(services, Enabled, new EnvironmentStub()));
        using var verify = services.CreateScope();
        var db = verify.ServiceProvider.GetRequiredService<AuthContext>();
        Assert.Empty(await db.Users.ToListAsync());
        Assert.Empty(await db.UserRoles.ToListAsync());
        Assert.Empty(await db.AuditEvents.ToListAsync());
    }

    private static ServiceProvider BuildServices(string connection)
    {
        var registrations = new ServiceCollection();
        registrations.AddLogging();
        registrations.AddHttpContextAccessor();
        registrations.AddDbContext<AuthContext>(options => options.UseNpgsql(connection).UseOpenIddict());
        registrations.AddIdentityCore<ApplicationUser>(options => options.Password.RequiredLength = 10)
            .AddRoles<ApplicationRole>().AddEntityFrameworkStores<AuthContext>();
        registrations.AddScoped<UserAdministration>();
        return registrations.BuildServiceProvider();
    }

    internal static IConfiguration Configuration(string enabled, string? password = null) =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DevelopmentSeed:Enabled"] = enabled, ["DevelopmentSeed:Password"] = password
        }).Build();

    internal sealed class EnvironmentStub(string name = "Development") : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "AuthSeedTests";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}

public sealed class DevelopmentSeedGuardTests
{
    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public async Task RejectsOutsideDevelopmentBeforeOpeningDatabase(string environment)
    {
        await using var services = new ServiceCollection().BuildServiceProvider();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => DevelopmentSeed.Initialize(services,
            DevelopmentSeedTests.Configuration("true"), new DevelopmentSeedTests.EnvironmentStub(environment)));
        Assert.Contains("Development", error.Message);
    }

    [Fact]
    public async Task DisabledSeedDoesNothingInProduction()
    {
        await using var services = new ServiceCollection().BuildServiceProvider();
        await DevelopmentSeed.Initialize(services, DevelopmentSeedTests.Configuration("false"), new DevelopmentSeedTests.EnvironmentStub("Production"));
    }

    [Theory]
    [InlineData("AppUsers")]
    [InlineData("vacinas")]
    [InlineData("vet_auth_tests_invalid")]
    [InlineData("vet_control_auth_dev_extra")]
    public async Task RejectsApplicationDatabasesBeforeConnecting(string database)
    {
        var registrations = new ServiceCollection();
        registrations.AddDbContext<AuthContext>(options => options.UseNpgsql($"Host=does-not-exist.invalid;Database={database};Username=unused;Password=unused").UseOpenIddict());
        await using var services = registrations.BuildServiceProvider();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => DevelopmentSeed.Initialize(services,
            DevelopmentSeedTests.Configuration("true"), new DevelopmentSeedTests.EnvironmentStub()));
        Assert.Contains("bancos fora", error.Message);
    }
}
