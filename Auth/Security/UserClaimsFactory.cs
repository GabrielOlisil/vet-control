using System.Security.Claims;
using Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Auth.Security;

public sealed class UserClaimsFactory(UserManager<ApplicationUser> users, RoleManager<ApplicationRole> roles,
    IOptions<IdentityOptions> options) : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>(users, roles, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        var roleType = Options.ClaimsIdentity.RoleClaimType;
        foreach (var role in RoleCatalog.Expand(await UserManager.GetRolesAsync(user)))
            if (!identity.HasClaim(roleType, role)) identity.AddClaim(new Claim(roleType, role));
        identity.AddClaim(new Claim("display_name", user.FullName));
        if (user.MustChangePassword) identity.AddClaim(new Claim("must_change_password", "true"));
        return identity;
    }
}
