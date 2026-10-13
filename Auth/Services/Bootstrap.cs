using Auth.Models;
using Auth.Security;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Auth.Services;

public static class Bootstrap
{
    public static async Task Initialize(IServiceProvider services, IConfiguration config)
    {
        using var scope = services.CreateScope();
        // Schema changes are applied explicitly with dotnet ef database update.
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        foreach (var (name, description) in RoleCatalog.Descriptions)
            if (!await roles.RoleExistsAsync(name)) Ensure(await roles.CreateAsync(new ApplicationRole { Name = name, Description = description }));

        var applications = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = config["OpenId:ClientId"]!, ClientSecret = config["OpenId:ClientSecret"]
                ?? throw new InvalidOperationException("Configure OpenId:ClientSecret."),
            ClientType = ClientTypes.Confidential, ConsentType = ConsentTypes.Implicit,
            DisplayName = "Vet Control",
            RedirectUris = { new Uri(config["OpenId:RedirectUri"]!) },
            PostLogoutRedirectUris = { new Uri(config["OpenId:PostLogoutRedirectUri"]!) },
            Permissions =
            {
                Permissions.Endpoints.Authorization, Permissions.Endpoints.Token, Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode, Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code, Permissions.Scopes.Profile, Permissions.Scopes.Email,
                Permissions.Scopes.Roles, Permissions.Prefixes.Scope + "vet-control-api"
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange }
        };
        var application = await applications.FindByClientIdAsync(descriptor.ClientId);
        if (application is null) await applications.CreateAsync(descriptor);
        else await applications.UpdateAsync(application, descriptor);

    }
    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(" ", result.Errors.Select(x => x.Description)));
    }
}
