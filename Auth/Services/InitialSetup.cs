using Auth.Contracts;
using Auth.Database;
using Auth.Models;
using Auth.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services;

public sealed class InitialSetup(AuthContext db, UserManager<ApplicationUser> users, UserAdministration audit)
{
    public async Task<bool> IsRequired(CancellationToken cancellationToken = default) =>
        !await db.Users.AnyAsync(cancellationToken);

    public async Task<ApplicationUser> CreateAdministrator(InitialSetupRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        // Serialize first-account creation across requests and API instances, not just this process.
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(1948893432087)", cancellationToken);
        if (!await IsRequired(cancellationToken))
            throw new UserOperationException("A configuração inicial já foi concluída. Entre com uma conta existente.", 409);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(), FullName = request.FullName.Trim(), UserName = request.UserName.Trim(),
            Email = request.Email.Trim(), EmailConfirmed = true, IsActive = true, MustChangePassword = false,
            LastLoginAt = DateTimeOffset.UtcNow
        };
        Ensure(await users.CreateAsync(user, request.Password));
        Ensure(await users.AddToRoleAsync(user, RoleCatalog.Admin));
        audit.Record(user.Id, "user.bootstrap", after: await audit.Describe(user), actorId: user.Id);
        audit.Record(user.Id, "login.succeeded", actorId: user.Id);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return user;
    }

    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded) throw new UserOperationException(string.Join(" ", result.Errors.Select(x => x.Description)));
    }
}
