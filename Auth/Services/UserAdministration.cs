using System.Data;
using System.Security.Claims;
using System.Text.Json;
using Auth.Contracts;
using Auth.Database;
using Auth.Models;
using Auth.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services;

public sealed class UserOperationException(string message, int statusCode = 400) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public sealed class UserAdministration(AuthContext db, UserManager<ApplicationUser> users, IHttpContextAccessor accessor)
{
    public async Task<UserResponse> Describe(ApplicationUser user)
    {
        var roles = (await users.GetRolesAsync(user)).Order().ToArray();
        return new(user.Id, user.FullName, user.UserName!, user.Email!, user.IsActive, user.MustChangePassword,
            await users.IsLockedOutAsync(user), user.LockoutEnd, user.LastLoginAt, user.CreationDateTime,
            user.LastModificationDateTime, user.ConcurrencyStamp!, roles, RoleCatalog.Expand(roles), user.Crmv, user.CrmvUf);
    }

    public async Task<UserPageResponse> List(string? search, bool? active, string? role, int page, int pageSize)
    {
        page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100);
        var query = users.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = "%" + search.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
            query = query.Where(x => EF.Functions.ILike(x.FullName, pattern) ||
                EF.Functions.ILike(x.UserName!, pattern) || EF.Functions.ILike(x.Email!, pattern));
        }
        if (active.HasValue) query = query.Where(x => x.IsActive == active.Value);
        if (!string.IsNullOrEmpty(role))
        {
            if (!RoleCatalog.Descriptions.ContainsKey(role)) throw new UserOperationException("Role inválida.");
            var included = role is RoleCatalog.Handler or RoleCatalog.Applicator;
            query = query.Where(x => (from ur in db.UserRoles join r in db.Roles on ur.RoleId equals r.Id
                where ur.UserId == x.Id && (r.Name == role || included && r.Name == RoleCatalog.Intern) select ur).Any());
        }
        var total = await query.CountAsync();
        var list = await query.OrderBy(x => x.FullName).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var responses = new List<UserResponse>();
        foreach (var user in list) responses.Add(await Describe(user));
        return new(responses.ToArray(), total, page, pageSize);
    }

    public Task<ApplicationUser> Find(Guid id) => FindCore(id);
    private async Task<ApplicationUser> FindCore(Guid id) => await users.FindByIdAsync(id.ToString())
        ?? throw new UserOperationException("Usuário não encontrado.", 404);

    private static string[] ValidateRoles(string[]? roles, bool isActive)
    {
        var values = roles?.Distinct(StringComparer.Ordinal).ToArray() ?? [];
        if (values.Any(x => !RoleCatalog.Descriptions.ContainsKey(x))) throw new UserOperationException("Uma das roles é inválida.");
        if (isActive && values.Length == 0) throw new UserOperationException("Selecione pelo menos um papel para manter a conta ativa.");
        return values;
    }

    private static void Check(IdentityResult result)
    {
        if (!result.Succeeded) throw new UserOperationException(string.Join(" ", result.Errors.Select(x => x.Description)),
            result.Errors.Any(x => x.Code == "ConcurrencyFailure") ? 409 : 400);
    }

    private static void CheckVersion(ApplicationUser user, string version)
    {
        if (user.ConcurrencyStamp != version) throw new UserOperationException("O usuário foi alterado por outra solicitação. Recarregue antes de salvar.", 409);
    }

    public void Record(Guid? userId, string action, object? before = null, object? after = null, Guid? actorId = null)
    {
        var actorClaim = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        db.AuditEvents.Add(new UserAuditEvent
        {
            UserId = userId, ActorId = actorId ?? (Guid.TryParse(actorClaim, out var id) ? id : null),
            Action = action, BeforeJson = before is null ? null : JsonSerializer.Serialize(AuditSnapshot(before)),
            AfterJson = after is null ? null : JsonSerializer.Serialize(AuditSnapshot(after)),
            IpAddress = accessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
        });
    }

    private static object AuditSnapshot(object value) => value is UserResponse user ? new
    {
        user.Id, user.FullName, user.UserName, user.Email, user.IsActive, user.MustChangePassword,
        user.IsLockedOut, user.LockoutEnd, user.Roles, user.EffectiveRoles, user.Crmv, user.CrmvUf
    } : value;

    public async Task<UserResponse> Create(CreateUserRequest request)
    {
        var roles = ValidateRoles(request.Roles, request.IsActive);
        var (crmv, crmvUf) = NormalizeProfessionalRegistration(roles, request.Crmv, request.CrmvUf);
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = new ApplicationUser { Id = Guid.NewGuid(), FullName = request.FullName.Trim(), UserName = request.UserName.Trim(),
            Email = request.Email.Trim(), EmailConfirmed = true, IsActive = request.IsActive, MustChangePassword = request.MustChangePassword,
            Crmv = crmv, CrmvUf = crmvUf };
        Check(await users.CreateAsync(user, request.Password));
        Check(await users.AddToRolesAsync(user, roles));
        var response = await Describe(user);
        Record(user.Id, "user.created", after: response);
        await db.SaveChangesAsync(); await transaction.CommitAsync();
        return response;
    }

    public async Task<UserResponse> Update(Guid id, UpdateUserRequest request)
    {
        var roles = ValidateRoles(request.Roles, request.IsActive);
        var (crmv, crmvUf) = NormalizeProfessionalRegistration(roles, request.Crmv, request.CrmvUf);
        // Serialize changes that might remove the last active administrator.
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var user = await Find(id); CheckVersion(user, request.ConcurrencyStamp);
        var before = await Describe(user);
        var currentRoles = await users.GetRolesAsync(user);
        if (user.IsActive && currentRoles.Contains(RoleCatalog.Admin) && (!request.IsActive || !roles.Contains(RoleCatalog.Admin)))
        {
            var adminRole = await db.Roles.SingleAsync(x => x.Name == RoleCatalog.Admin);
            var others = await (from u in db.Users join ur in db.UserRoles on u.Id equals ur.UserId
                where u.IsActive && u.Id != user.Id && ur.RoleId == adminRole.Id select u.Id).AnyAsync();
            if (!others) throw new UserOperationException("É necessário manter pelo menos um administrador ativo.");
        }
        user.FullName = request.FullName.Trim(); user.UserName = request.UserName.Trim();
        user.Email = request.Email.Trim(); user.EmailConfirmed = true; user.IsActive = request.IsActive;
        user.Crmv = crmv; user.CrmvUf = crmvUf;
        user.SessionVersion = Guid.NewGuid().ToString("N");
        Check(await users.UpdateAsync(user));
        Check(await users.RemoveFromRolesAsync(user, currentRoles.Except(roles)));
        Check(await users.AddToRolesAsync(user, roles.Except(currentRoles)));
        Check(await users.UpdateSecurityStampAsync(user));
        var after = await Describe(user); Record(user.Id, "user.updated", before, after);
        await db.SaveChangesAsync(); await transaction.CommitAsync();
        return after;
    }

    private static (string? Crmv, string? CrmvUf) NormalizeProfessionalRegistration(
        IReadOnlyCollection<string> roles, string? crmv, string? crmvUf)
    {
        var normalizedCrmv = string.IsNullOrWhiteSpace(crmv) ? null : crmv.Trim();
        var normalizedUf = string.IsNullOrWhiteSpace(crmvUf) ? null : crmvUf.Trim().ToUpperInvariant();

        if (normalizedUf is not null && normalizedUf.Length != 2)
            throw new UserOperationException("A UF do CRMV deve possuir exatamente 2 caracteres.");

        if (roles.Contains(RoleCatalog.Veterinarian) &&
            (string.IsNullOrWhiteSpace(normalizedCrmv) || string.IsNullOrWhiteSpace(normalizedUf)))
        {
            throw new UserOperationException("Usuários com a role medico_veterinario precisam possuir CRMV e UF do CRMV cadastrados.");
        }

        return (normalizedCrmv, normalizedUf);
    }

    public async Task<UserResponse> ResetPassword(Guid id, ResetPasswordRequest request)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = await Find(id); CheckVersion(user, request.ConcurrencyStamp);
        Check(await users.ResetPasswordAsync(user, await users.GeneratePasswordResetTokenAsync(user), request.NewPassword));
        user.MustChangePassword = request.MustChangePassword; user.SessionVersion = Guid.NewGuid().ToString("N");
        Check(await users.UpdateAsync(user));
        Record(id, "user.password_reset", after: new { request.MustChangePassword });
        await db.SaveChangesAsync(); await transaction.CommitAsync();
        return await Describe(user);
    }

    public async Task<UserResponse> Unlock(Guid id, VersionRequest request)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = await Find(id); CheckVersion(user, request.ConcurrencyStamp);
        Check(await users.SetLockoutEndDateAsync(user, null)); Check(await users.ResetAccessFailedCountAsync(user));
        Record(id, "user.unlocked"); await db.SaveChangesAsync(); await transaction.CommitAsync();
        return await Describe(user);
    }
}
