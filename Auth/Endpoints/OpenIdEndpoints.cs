using System.Security.Claims;
using Auth.Models;
using Auth.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Auth.Endpoints;

public static class OpenIdEndpoints
{
    public static void MapOpenIdEndpoints(this WebApplication app)
    {
        app.MapMethods("/connect/authorize", ["GET", "POST"], Authorize).AllowAnonymous().ExcludeFromDescription();
        app.MapPost("/connect/token", Exchange).AllowAnonymous().ExcludeFromDescription();
        app.MapMethods("/connect/userinfo", ["GET", "POST"], UserInfo).AllowAnonymous().ExcludeFromDescription();
        app.MapMethods("/connect/logout", ["GET", "POST"], Logout).AllowAnonymous().ExcludeFromDescription();
    }

    private static async Task<IResult> Authorize(HttpContext context, UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signIn, IConfiguration config)
    {
        var request = context.GetOpenIddictServerRequest()!;
        var session = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        var user = session.Succeeded ? await users.GetUserAsync(session.Principal!) : null;
        var stampValid = user is not null && await signIn.ValidateSecurityStampAsync(session.Principal!) is not null;
        if (!stampValid || user is null || !user.IsActive || user.MustChangePassword || await users.IsLockedOutAsync(user) ||
            request.HasPromptValue(PromptValues.Login))
        {
            if (request.HasPromptValue(PromptValues.None)) return Reject(Errors.LoginRequired, "Uma sessão válida é necessária.");
            // The authorization request was already validated by OpenIddict before this handler.
            var returnUrl = context.Request.Path + context.Request.QueryString;
            return Results.Redirect(config["Frontend:Origin"] + "/auth/login?returnUrl=" + Uri.EscapeDataString(returnUrl));
        }
        if (request.MaxAge is not null && session.Properties?.IssuedUtc is { } issued &&
            DateTimeOffset.UtcNow - issued > TimeSpan.FromSeconds(request.MaxAge.Value))
        {
            if (request.HasPromptValue(PromptValues.None)) return Reject(Errors.LoginRequired, "A sessão precisa ser renovada.");
            return Results.Redirect(config["Frontend:Origin"] + "/auth/login?returnUrl=" +
                Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
        }
        return Results.SignIn(await Principal(user, request.GetScopes(), users), authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static async Task<IResult> Exchange(HttpContext context, UserManager<ApplicationUser> users)
    {
        var request = context.GetOpenIddictServerRequest()!;
        if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
            return Reject(Errors.UnsupportedGrantType, "Fluxo não suportado.");
        var auth = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var principal = auth.Principal;
        var user = principal is null ? null : await users.FindByIdAsync(principal.GetClaim(Claims.Subject)!);
        if (user is null || !user.IsActive || user.MustChangePassword || await users.IsLockedOutAsync(user) ||
            principal!.GetClaim("session_version") != user.SessionVersion)
            return Reject(Errors.InvalidGrant, "Sessão revogada. Entre novamente.");
        return Results.SignIn(await Principal(user, principal!.GetScopes(), users), authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static async Task<ClaimsPrincipal> Principal(ApplicationUser user, IEnumerable<string> scopes, UserManager<ApplicationUser> users)
    {
        var identity = new ClaimsIdentity("OpenIddict", Claims.Name, Claims.Role);
        identity.SetClaim(Claims.Subject, user.Id.ToString());
        identity.SetClaim(Claims.Name, user.FullName);
        identity.SetClaim(Claims.PreferredUsername, user.UserName);
        identity.SetClaim(Claims.Email, user.Email);
        identity.SetClaim("session_version", user.SessionVersion);
        foreach (var role in RoleCatalog.Expand(await users.GetRolesAsync(user))) identity.AddClaim(new Claim(Claims.Role, role));
        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(scopes); principal.SetResources("vet-control-api");
        principal.SetDestinations(claim => claim.Type switch
        {
            Claims.Name or Claims.PreferredUsername when principal.HasScope(Scopes.Profile) => [Destinations.AccessToken, Destinations.IdentityToken],
            Claims.Email when principal.HasScope(Scopes.Email) => [Destinations.AccessToken, Destinations.IdentityToken],
            Claims.Role when principal.HasScope(Scopes.Roles) => [Destinations.AccessToken, Destinations.IdentityToken],
            "session_version" => [Destinations.AccessToken],
            _ => []
        });
        return principal;
    }

    private static async Task<IResult> UserInfo(HttpContext context, UserManager<ApplicationUser> users)
    {
        var auth = await context.AuthenticateAsync(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        if (!auth.Succeeded) return Results.Challenge(authenticationSchemes: [OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme]);
        var user = await users.FindByIdAsync(auth.Principal!.GetClaim(Claims.Subject)!);
        if (user is null || !user.IsActive || user.MustChangePassword || await users.IsLockedOutAsync(user) ||
            auth.Principal.GetClaim("session_version") != user.SessionVersion) return Results.Unauthorized();
        var claims = new Dictionary<string, object?> { [Claims.Subject] = user.Id.ToString() };
        if (auth.Principal.HasScope(Scopes.Profile))
        { claims[Claims.Name] = user.FullName; claims[Claims.PreferredUsername] = user.UserName; }
        if (auth.Principal.HasScope(Scopes.Email)) claims[Claims.Email] = user.Email;
        if (auth.Principal.HasScope(Scopes.Roles)) claims[Claims.Role] = RoleCatalog.Expand(await users.GetRolesAsync(user));
        return Results.Ok(claims);
    }

    private static async Task<IResult> Logout(HttpContext context, SignInManager<ApplicationUser> signIn)
    {
        await signIn.SignOutAsync();
        return Results.SignOut(authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }

    private static IResult Reject(string error, string message) => Results.Forbid(new AuthenticationProperties(new Dictionary<string, string?>
    {
        [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = message
    }), [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
}
