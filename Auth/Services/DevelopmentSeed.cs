using System.Text.RegularExpressions;
using Auth.Database;
using Auth.Models;
using Auth.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Auth.Services;

/// <summary>Explicit, isolated demonstration accounts; never runs against application databases.</summary>
public static partial class DevelopmentSeed
{
    private const string CompletedAction = "development.seed.v1.completed";

    public static async Task Initialize(IServiceProvider services, IConfiguration config, IHostEnvironment environment)
    {
        if (!config.GetValue<bool>("DevelopmentSeed:Enabled")) return;
        if (!environment.IsDevelopment())
            throw new InvalidOperationException("DevelopmentSeed só pode ser habilitado em Development.");

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthContext>();
        var database = new NpgsqlConnectionStringBuilder(db.Database.GetConnectionString()).Database ?? "";
        if (database != "vet_control_auth_dev" && !DisposableIdentityDatabase().IsMatch(database))
            throw new InvalidOperationException("DevelopmentSeed recusa bancos fora do ambiente isolado de demonstração.");

        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(1948893432087)");
        // This durable audit marker is written in the same transaction as both accounts.
        // A restart preserves edited passwords, disabled accounts and additional users.
        if (await db.AuditEvents.AnyAsync(x => x.Action == CompletedAction)) return;
        if (await db.Users.AnyAsync())
            throw new InvalidOperationException("DevelopmentSeed recusa uma base com usuários existentes sem o marcador de demonstração.");

        var password = config["DevelopmentSeed:Password"] ?? "Demonstracao123!";
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var audit = scope.ServiceProvider.GetRequiredService<UserAdministration>();
        foreach (var account in new[]
        {
            (UserName: "admin", FullName: "Administrador de demonstração", Role: RoleCatalog.Admin, Crmv: (string?)null, Uf: (string?)null),
            (UserName: "veterinario", FullName: "Veterinário de demonstração", Role: RoleCatalog.Veterinarian, Crmv: "DEMO-001", Uf: "AM")
        })
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(), UserName = account.UserName, FullName = account.FullName,
                Email = account.UserName + "@demo.example.invalid", EmailConfirmed = true,
                IsActive = true, MustChangePassword = false, Crmv = account.Crmv, CrmvUf = account.Uf
            };
            Ensure(await users.CreateAsync(user, password));
            Ensure(await users.AddToRoleAsync(user, account.Role));
            audit.Record(user.Id, "development.user.created", after: await audit.Describe(user));
        }
        audit.Record(null, CompletedAction);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(x => x.Description)));
    }

    [GeneratedRegex("\\Avet_auth_tests_[a-f0-9]{32}\\z", RegexOptions.CultureInvariant)]
    private static partial Regex DisposableIdentityDatabase();
}
