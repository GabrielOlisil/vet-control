using System.Security.Claims;
using Auth.Contracts;
using Auth.Database;
using Auth.Models;
using Auth.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace Auth.Endpoints;

public sealed class CsrfFilter(IAntiforgery antiforgery) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try { await antiforgery.ValidateRequestAsync(context.HttpContext); }
        catch (AntiforgeryValidationException) { return Results.Problem("A sessão do formulário expirou. Recarregue a página.", statusCode: 400); }
        return await next(context);
    }
}

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        var account = app.MapGroup("/api/auth").WithTags("Conta");
        account.MapGet("/setup", async (InitialSetup setup, CancellationToken cancellationToken) =>
            new SetupStatusResponse(await setup.IsRequired(cancellationToken))).AllowAnonymous();
        account.MapPost("/setup", async (InitialSetupRequest request, InitialSetup setup, SignInManager<ApplicationUser> signIn,
            IConfiguration config, CancellationToken cancellationToken) =>
        {
            var user = await setup.CreateAdministrator(request, cancellationToken);
            await signIn.SignInAsync(user, isPersistent: false);
            return Results.Ok(new LoginResponse(NextUrl(request.ReturnUrl, config), MustChangePassword: false));
        }).AllowAnonymous().Produces<LoginResponse>().RequireRateLimiting("login")
            .AddEndpointFilter<CsrfFilter>().AddEndpointFilter<RequestValidationFilter>();
        account.MapGet("/session", async (HttpContext context, IAntiforgery csrf, UserManager<ApplicationUser> users) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            var token = csrf.GetAndStoreTokens(context).RequestToken!;
            var user = await users.GetUserAsync(context.User);
            return Results.Ok(new AuthSessionResponse(user is not null, user?.Id, user?.FullName, user?.UserName, user?.Email,
                user is null ? [] : Auth.Security.RoleCatalog.Expand(await users.GetRolesAsync(user)), user?.MustChangePassword ?? false, token));
        }).AllowAnonymous().Produces<AuthSessionResponse>();

        account.MapPost("/login", async (LoginRequest request, HttpContext context, UserManager<ApplicationUser> users,
            SignInManager<ApplicationUser> signIn, AuthContext db, UserAdministration admin, IConfiguration config) =>
        {
            var user = await users.FindByNameAsync(request.Login.Trim()) ?? await users.FindByEmailAsync(request.Login.Trim());
            if (user is null)
            {
                // Perform a real hash verification even for unknown accounts.
                _ = users.PasswordHasher.HashPassword(new ApplicationUser(), request.Password);
                return Results.Problem("Usuário ou senha inválidos.", statusCode: 401);
            }
            if (!user.IsActive) return Results.Problem("Usuário ou senha inválidos.", statusCode: 401);
            var result = await signIn.PasswordSignInAsync(user, request.Password, false, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                admin.Record(user.Id, "login.failed", actorId: user.Id); await db.SaveChangesAsync();
                return Results.Problem("Usuário ou senha inválidos, ou conta temporariamente bloqueada.", statusCode: 401);
            }
            user.LastLoginAt = DateTimeOffset.UtcNow;
            var updated = await users.UpdateAsync(user);
            if (!updated.Succeeded) { await signIn.SignOutAsync(); return Results.Problem("Tente entrar novamente.", statusCode: 409); }
            admin.Record(user.Id, "login.succeeded", actorId: user.Id); await db.SaveChangesAsync();
            var nextUrl = NextUrl(request.ReturnUrl, config);
            return Results.Ok(new LoginResponse(nextUrl, user.MustChangePassword));
        }).AllowAnonymous().Produces<LoginResponse>().RequireRateLimiting("login").AddEndpointFilter<CsrfFilter>().AddEndpointFilter<RequestValidationFilter>();

        account.MapPost("/logout", async (HttpContext context, SignInManager<ApplicationUser> signIn, AuthContext db, UserAdministration admin) =>
        {
            if (Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) admin.Record(id, "logout");
            await signIn.SignOutAsync(); await db.SaveChangesAsync(); return Results.NoContent();
        }).RequireAuthorization().AddEndpointFilter<CsrfFilter>();

        account.MapPost("/password", async (PasswordChangeRequest request, HttpContext context, UserManager<ApplicationUser> users,
            SignInManager<ApplicationUser> signIn, AuthContext db, UserAdministration admin) =>
        {
            var user = await users.GetUserAsync(context.User);
            if (user is null) return Results.Unauthorized();
            await using var transaction = await db.Database.BeginTransactionAsync();
            var result = await users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded) return Results.Problem(string.Join(" ", result.Errors.Select(x => x.Description)), statusCode: 400);
            user.MustChangePassword = false; user.SessionVersion = Guid.NewGuid().ToString("N");
            result = await users.UpdateAsync(user);
            if (!result.Succeeded) return Results.Problem("A conta foi alterada. Tente novamente.", statusCode: 409);
            admin.Record(user.Id, "user.password_changed"); await db.SaveChangesAsync(); await transaction.CommitAsync();
            await signIn.RefreshSignInAsync(user);
            return Results.NoContent();
        }).RequireAuthorization().AddEndpointFilter<CsrfFilter>().AddEndpointFilter<RequestValidationFilter>();
    }

    // Never accept arbitrary redirects from a login request.
    public static string NextUrl(string? returnUrl, IConfiguration config)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Uri.TryCreate(returnUrl, UriKind.Relative, out _) &&
            returnUrl.StartsWith("/connect/authorize?", StringComparison.Ordinal))
        {
            var values = QueryHelpers.ParseQuery(returnUrl[(returnUrl.IndexOf('?') + 1)..]);
            // Reauthentication has just taken place. Preserve state, nonce and PKCE parameters.
            values.Remove("prompt"); values.Remove("max_age");
            return QueryHelpers.AddQueryString(config["OpenId:Issuer"]!.TrimEnd('/') + "/connect/authorize", values);
        }
        return config["Frontend:Origin"] + "/auth";
    }
}
