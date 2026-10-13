using Auth.Contracts;
using Auth.Database;
using Auth.Security;
using Auth.Services;
using Microsoft.EntityFrameworkCore;

namespace Auth.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth/admin").RequireAuthorization("UserAdministration").WithTags("Administração de usuários");
        group.MapGet("/roles", () => RoleCatalog.Descriptions.Select(x => new RoleResponse(x.Key, x.Value,
            x.Key == RoleCatalog.Intern ? [RoleCatalog.Applicator, RoleCatalog.Handler] : [])));
        group.MapGet("/users", (UserAdministration service, string? search, bool? active, string? role, int page = 1, int pageSize = 20) =>
            service.List(search, active, role, page, pageSize));
        group.MapGet("/users/{id:guid}", async (Guid id, UserAdministration service) => await service.Describe(await service.Find(id)));
        group.MapPost("/users", async (CreateUserRequest request, UserAdministration service) =>
        {
            var user = await service.Create(request); return Results.Created($"/api/auth/admin/users/{user.Id}", user);
        }).Produces<UserResponse>(201).AddEndpointFilter<CsrfFilter>().AddEndpointFilter<RequestValidationFilter>();
        group.MapPut("/users/{id:guid}", (Guid id, UpdateUserRequest request, UserAdministration service) => service.Update(id, request))
            .AddEndpointFilter<CsrfFilter>().AddEndpointFilter<RequestValidationFilter>();
        group.MapPost("/users/{id:guid}/reset-password", (Guid id, ResetPasswordRequest request, UserAdministration service) => service.ResetPassword(id, request))
            .AddEndpointFilter<CsrfFilter>().AddEndpointFilter<RequestValidationFilter>();
        group.MapPost("/users/{id:guid}/unlock", (Guid id, VersionRequest request, UserAdministration service) => service.Unlock(id, request))
            .AddEndpointFilter<CsrfFilter>().AddEndpointFilter<RequestValidationFilter>();
        group.MapGet("/users/{id:guid}/audit", async (Guid id, AuthContext db, UserAdministration service, int page = 1) =>
        {
            _ = await service.Find(id); page = Math.Max(page, 1);
            var query = db.AuditEvents.AsNoTracking().Where(x => x.UserId == id);
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(x => x.CreationDateTime).ThenBy(x => x.Id).Skip((page - 1) * 20).Take(20)
                .Select(x => new AuditEventResponse(x.Id, x.Action, x.ActorId,
                    db.Users.Where(u => u.Id == x.ActorId).Select(u => u.FullName).FirstOrDefault(),
                    x.IpAddress, x.BeforeJson, x.AfterJson, x.CreationDateTime)).ToArrayAsync();
            return Results.Ok(new AuditPageResponse(items, total, page, 20));
        }).Produces<AuditPageResponse>();
    }
}
