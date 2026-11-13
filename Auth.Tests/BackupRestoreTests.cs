using Auth.Database;
using Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetControl.Testing;
using static VetControl.Testing.BackupTool;
using Xunit;

namespace Auth.Tests;

public sealed class BackupRestoreTests
{
    [Fact]
    public async Task IdentityDumpRestoresPasswordsRolesAndAuditWithoutTouchingApplicationDatabases()
    {
        await using var source = new PostgresTestDatabase("auth");
        await using var target = new PostgresTestDatabase("auth");
        await source.CreateAsync();
        await target.CreateAsync();
        await using var db = Context(source);
        await db.Database.MigrateAsync();
        var user = new ApplicationUser { Id = Guid.NewGuid(), FullName = "Admin restaurável", UserName = "restore-admin", NormalizedUserName = "RESTORE-ADMIN", Email = "restore@test.local", NormalizedEmail = "RESTORE@TEST.LOCAL", SecurityStamp = Guid.NewGuid().ToString() };
        user.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(user, "Backup-Test1!");
        var role = new ApplicationRole { Id = Guid.NewGuid(), Name = "admin", NormalizedName = "ADMIN" };
        db.AddRange(user, role, new IdentityUserRole<Guid> { UserId = user.Id, RoleId = role.Id },
            new UserAuditEvent { UserId = user.Id, ActorId = user.Id, Action = "BackupFixture", AfterJson = "{\"isActive\":true}" });
        await db.SaveChangesAsync();
        var root = FindRoot();
        var directory = Path.Combine(root, "artifacts", "tests", target.Name);
        Directory.CreateDirectory(directory);
        var file = Path.Combine(directory, "identity.dump");
        var container = await ContainerAsync();
        foreach (var operation in new[] { "backup", "restore" })
        {
            var database = operation == "backup" ? source.Name : target.Name;
            PostgresTestDatabase.ValidateName(database);
            var result = await Run("node", ["deployment/database-backup.mjs", operation, "--container", container,
                "--database", database, "--file", file, "--expected-database", target.Name], root);
            Assert.True(result.Exit == 0, result.Output);
        }
        await using var restored = Context(target);
        Assert.Empty(await restored.Database.GetPendingMigrationsAsync());
        var savedUser = await restored.Users.SingleAsync();
        Assert.Equal(user.Id, savedUser.Id);
        Assert.Equal(user.SessionVersion, savedUser.SessionVersion);
        Assert.NotEqual(PasswordVerificationResult.Failed,
            new PasswordHasher<ApplicationUser>().VerifyHashedPassword(savedUser, savedUser.PasswordHash!, "Backup-Test1!"));
        Assert.Equal(role.Id, (await restored.UserRoles.SingleAsync()).RoleId);
        Assert.Equal("BackupFixture", (await restored.AuditEvents.SingleAsync()).Action);
    }

    private static AuthContext Context(PostgresTestDatabase database) => new(new DbContextOptionsBuilder<AuthContext>()
        .UseNpgsql(database.ConnectionString).UseOpenIddict().Options);
}
